using Cinemachine;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCar : MonoBehaviour
{
    [Header("References")]


    [Header("Car Status")]
    private int playerNumber = 0;
    private bool isGrounded = false;  // Whether the car is on the ground (requires 1 wheel on the ground)



    [Header("Car Rotation")]
    [SerializeField] private float carRotationMaxX = 60f;  // The car's maximum X rotation
    [SerializeField] private float carRotationMaxZ = 60f;  // The car's maximum Z rotation
    [SerializeField] private AnimationCurve airRotationCurve;  // How quickly the car returns to defualt rotation while in the air

    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;  // How quickly tires will spin at maxSpeed
    [SerializeField] private float maxTireSteeringAngle = 30f;  // How far the front tires will rotate when steering
    [SerializeField] private float minSideSkidVelocity = 10f;  // How fast you need to be sliding for skid marks to appear on rear tires
    [SerializeField, Range(0f, 5f)] private float modelRotationSpeed = 0.1f;  // How quickly the car model will rotate
    [SerializeField, Range(0, 160)] private float maxDriftModelTurnAngle = 30f;  // How far the car will turn when sharply brake drifting
    [SerializeField, Range(0, 160)] private float maxDriftModelTiltAngle = 30f;  // How far the car will turn when sharply brake drifting
    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[4];  // The four skid mark trail renderers
    [SerializeField] private float tireSuspensionMoveSpeed = 0.01f;  // How quickly the tires move up and down with the terrain
    private Vector3[] tirePositions = new Vector3[4];  // The starting positions for each tire
    private Quaternion targetCarRotation;  // The rotation that the car wants to reach

    [Header("Debug")]
    [SerializeField] private bool wheelRays = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        virtualCamera = Camera.main.transform.parent.GetComponent<CinemachineVirtualCamera>();

        canMove = true;
        UpdateDebugStats();
        SetCenterOfMass();

        if (playerNumber > 1) Destroy(virtualCamera.transform.GetChild(0).gameObject);

        for (int i = 0; i < tires.Length; i++) tirePositions[i] = tires[i].transform.parent.localPosition;
    }

    private void Update()
    {
        GetPlayerInput();
        Drift();
        UpdateDebugStats();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        Suspension();
        GroundCheck();
        BrakeCheck();
        CalculateCarVelocity();
        WallDetection();
        Movement();
        CarRotation();
        Visuals();
    }

    #region Car Status


    private void BrakeCheck() { isBraking = (moveInput < 0 && carVelocityRatio > 0 && isGrounded); }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(rb.linearVelocity).Round(2);

        if (currentCarLocalVelocity.z >= 0) carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
        else carVelocityRatio = currentCarLocalVelocity.z / maxReverseSpeed;

        carVelocityRatio = carVelocityRatio.Round(2);
        if (carVelocityRatio >= 0.975) carVelocityRatio = 1;
        if (carVelocityRatio <= -0.975) carVelocityRatio = -1;
    }

    private void SetCenterOfMass()
    {
        float x = 0;
        float z = 0;
        foreach (Transform point in rayPoints)
        {
            x += point.localPosition.x;
            z += point.localPosition.z;
        }
        rb.centerOfMass = new Vector3(x, -0.25f, z); // Lower the center of mass.
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        if (wallDetectionLayer.Contains(collision.gameObject.layer))  // When driving into a wall
        {
            WallBounceback(collision);
        }
    }

    private void WallDetection()
    {
        wallAngle = 0;

        float carSpeed = Mathf.Max(Mathf.Abs(carVelocityRatio), Mathf.Abs(moveInput));
        if (carSpeed <= 0.1f) return;  // If the car is not actually moving then don't redirect

        Transform lRayPoint, rRayPoint;
        if (carVelocityRatio >= 0.1f || moveInput > 0)  // If the car is driving forwards
        {
            lRayPoint = rayPoints[0];
            rRayPoint = rayPoints[1];
        }
        else  // If the car is reversing
        {
            lRayPoint = rayPoints[3];
            rRayPoint = rayPoints[2];
        }

        RaycastHit lHit, rHit;
        bool lDidHit = Physics.Raycast(lRayPoint.position, lRayPoint.forward, out lHit, wallDetectionDistance, wallDetectionLayer, QueryTriggerInteraction.Ignore);
        bool rDidHit = Physics.Raycast(rRayPoint.position, rRayPoint.forward, out rHit, wallDetectionDistance, wallDetectionLayer, QueryTriggerInteraction.Ignore);

        Debug.DrawRay(lRayPoint.position, lRayPoint.forward * wallDetectionDistance, lDidHit ? Color.red : Color.green);
        Debug.DrawRay(rRayPoint.position, rRayPoint.forward * wallDetectionDistance, rDidHit ? Color.red : Color.green);

        float lDist = Mathf.Infinity;
        float rDist = Mathf.Infinity;
        if (lDidHit) lDist = lHit.distance;
        if (rDidHit) rDist = rHit.distance;

        if (lDist == Mathf.Infinity && rDist == Mathf.Infinity)  // If both rays miss, perform a third ray in the centre
        {
            Vector3 mRayPoint = Vector3.Lerp(lRayPoint.position, rRayPoint.position, 0.5f);
            lDidHit = Physics.Raycast(mRayPoint, lRayPoint.forward, out lHit, wallDetectionDistance, wallDetectionLayer, QueryTriggerInteraction.Ignore);

            Debug.DrawRay(mRayPoint, lRayPoint.forward * wallDetectionDistance, lDidHit ? Color.red : Color.green);

            if (lDidHit) lDist = lHit.distance;  // Third ray will count as the left ray hitting, so will steer right
            if (lDist == Mathf.Infinity) return;
        }

        // Figure out which direction the car should turn
        bool lCloser = lDist == Mathf.Min(lDist, rDist);
        float closerDist = lCloser ? lDist : rDist;
        float turnAmount = (closerDist / wallDetectionDistance);
        if (!lCloser) turnAmount *= -1;

        wallAngle = (Vector3.Angle(lCloser ? lRayPoint.forward : rRayPoint.forward, lCloser ? lHit.normal : rHit.normal) - 90) / 90;  // 1 for head on, 0 for parallel

        rb.AddRelativeTorque(wallCollisionRedirectSpeed * turnAmount * wallAngle * transform.up, ForceMode.Acceleration);  // Turn the car to be parallel with the wall
    }

    void WallBounceback(Collision collision)  // When driving into a wall head-on
    {
        float bounceback = wallBouncebackAngle.Evaluate(wallAngle);

        if (collision.gameObject.TryGetComponent<BounceKartBack>(out BounceKartBack bounce)) bounceback = bounce.bouncePower;

        if (carVelocityRatio > 0 && bounceback > 0.1f)
        {
            Debug.Log("hit " + collision.gameObject.name + "  -  Hit angle-" + bounceback + "  -  Bounceback Force-" + bounceback * wallBouncebackPower * carVelocityRatio);
            rb.AddForce(bounceback * wallBouncebackPower * carVelocityRatio * -rb.transform.forward, ForceMode.Impulse);
        }
    }

    private void UpdateDebugStats()
    {
        if (playerNumber == 0)
        {
            DebugStats.carStats.Add(new CarDebugStats());
            playerNumber = DebugStats.carStats.Count;
        }

        DebugStats.carStats[playerNumber - 1].moveInput = moveInput;
        if (isDrifting) DebugStats.carStats[playerNumber - 1].steerInput = driftControl * driftDirection;
        else DebugStats.carStats[playerNumber - 1].steerInput = steerInput;
        DebugStats.carStats[playerNumber - 1].braking = isBraking;
        DebugStats.carStats[playerNumber - 1].currentCarLocalVelocity = currentCarLocalVelocity;
        DebugStats.carStats[playerNumber - 1].carVelocityRatio = carVelocityRatio;

        int gW = 0;
        for (int i = 0; i < groundedWheels.Length; i++) { gW += groundedWheels[i]; }
        DebugStats.carStats[playerNumber - 1].groundedWheels = gW;
    }
    #endregion

    #region Movement
    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        }
    }

    private void Acceleration()
    {
        isReversing = (moveInput < 0 && carVelocityRatio < 0);
        if (Mathf.Abs(currentCarLocalVelocity.z) >= (!isReversing ? maxSpeed : maxReverseSpeed)) return;  // If we are already at our max speed then stop


        Vector3 accelDirection = accelerationPoint.transform.forward;


        float accel = acceleration * accelerationCurve.Evaluate(carVelocityRatio);
        if (isReversing) accel = reverseAcceleration * reverseAccelerationCurve.Evaluate(carVelocityRatio);  // Reverse acceleration
        accel *= moveInput;  // Multiply by the player's input (1 fowards, -1 reverse)

        rb.AddForceAtPosition(accel * accelDirection, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        rb.AddForce((isBraking ? brakingDeceleration : deceleration) * carVelocityRatio * -rb.transform.forward, ForceMode.Acceleration);
    }

    private void Turn()
    {
        float turnAmount = steerInput;

        int newSteeringDirection = steerInput > 0 ? 1 : -1;
        if (turnAmount == 0 || steeringDirection != newSteeringDirection) steeringTime = 0;
        else steeringTime += Time.fixedDeltaTime;
        steeringDirection = newSteeringDirection;
        //Debug.Log("Steering Time - " + steeringTime);
        if (isDrifting)
            turnAmount = driftControl * driftDirection;

        rb.AddRelativeTorque(steerStrength * turnAmount * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void Drift()
    {
        if (Input.GetButtonDown("Jump") && !isDrifting && steerInput != 0 && carVelocityRatio >= minVelocityRatioToDrift)  // Start Drifting
        {
            isDrifting = true;
            driftDirection = steerInput > 0 ? 1 : -1;
        }

        if ((Input.GetButtonUp("Jump") || carVelocityRatio < minVelocityRatioToDrift) && isDrifting)  // Stop Drifting
        {
            isDrifting = false;
        }

        if (isDrifting)
        {
            float driftControlInput = driftControlCurve.Evaluate(driftDirection == 1 ? steerInput : -steerInput);
            if (IsBrakeDrifting()) // Brake Drifting
                driftControl = ExtensionMethods.Remap(driftControlInput, -1, 1, brakeDriftWideTurn, brakeDriftSharpTurn);
            else  // Standard Drifting
                driftControl = ExtensionMethods.Remap(driftControlInput, -1, 1, driftWideTurn, driftSharpTurn);
        }
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * (isBraking ? brakingDragCoefficient : dragCoefficient);

        Vector3 dragForce = transform.right * dragMagnitude;


        // Steering Swing
        if (steerSwingStrength != 0 && isGrounded)
        {
            float swingAmount = steerSwingStrength;
            if (!isDrifting)
            {
                swingAmount *= steeringSwingCurve.Evaluate(Mathf.Abs(carVelocityRatio) * Mathf.Abs(steerInput)) * steeringSwingTimeCurve.Evaluate(steeringTime / steerSwingTime);  // Calculate how much to swing out by when not drifting
            }
            else
            {
                swingAmount *= driftSwingCurve.Evaluate(driftControl);
            }

            if (isReversing) swingAmount = 0;  // Don't swing if reversing
            if ((!isDrifting && steerInput > 0) || (isDrifting && driftDirection > 0)) swingAmount *= -1;  // Flip the force if steering right

            Vector3 swingForce = transform.right * swingAmount;
            dragForce += swingForce;
        }


        rb.AddForceAtPosition(dragForce, rb.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void CarRotation()
    {
        Vector3 currentRotation = transform.localEulerAngles;

        // Convert Unity's 0�360 range to -180�180 for easier clamping
        currentRotation.x = currentRotation.x.NormalizeAngle();
        currentRotation.z = currentRotation.z.NormalizeAngle();

        // Clamp the car's rotation so it can't go too far
        currentRotation.x = Mathf.Clamp(currentRotation.x, -carRotationMaxX, carRotationMaxX);
        currentRotation.z = Mathf.Clamp(currentRotation.z, -carRotationMaxZ, carRotationMaxZ);

        // Returns the car to a flat rotation if it has been in the air too long
        if (!isGrounded)
        {
            if (currentRotation.x < -1)
                currentRotation.x += airRotationCurve.Evaluate(airTime);
            else if (currentRotation.x > 1)
                currentRotation.x -= airRotationCurve.Evaluate(airTime);

            if (currentRotation.z < -1)
                currentRotation.z += airRotationCurve.Evaluate(airTime);
            else if (currentRotation.z > 1)
                currentRotation.z -= airRotationCurve.Evaluate(airTime);
        }

        transform.localEulerAngles = currentRotation;
    }
    #endregion

    #region Suspension
   
    #endregion

    #region Visuals
    private void Visuals()
    {
        TireVisuals();
        VFX();
        TurnCarRotation();
    }

    private void TurnCarRotation()
    {
        if (!isGrounded) return;

        float turnAmount = steerInput;
        if (isDrifting) turnAmount = driftControl * driftDirection;
        float y = Mathf.InverseLerp(0, brakeDriftSharpTurn, Mathf.Abs(turnAmount));
        y = (y / 100) * (carVelocityRatio * 100);

        // Turn
        float targetTurn = Mathf.Lerp(0, maxDriftModelTurnAngle, y) * (isDrifting ? driftDirection : steerInput);

        // Tilt
        float targetTilt = Mathf.Lerp(0, maxDriftModelTiltAngle, y) * (isDrifting ? driftDirection : steerInput);
        if (isDrifting && !isBraking) targetTilt /= 4f;  // If standard drifting
        if (!isDrifting) targetTilt /= 3f;  // If not drifting

        targetCarRotation = Quaternion.Euler(new Vector3(0, targetTurn, targetTilt));

        carModel.localRotation = Quaternion.Lerp(carModel.localRotation, targetCarRotation, modelRotationSpeed * Time.fixedDeltaTime);
    }

    private void TireVisuals()
    {
        float steeringAngle = maxTireSteeringAngle * steerInput;

        for (int i = 0; i < tires.Length; i++)
        {
            if (i < 2)  // Front tires
            {
                tires[i].transform.Rotate(-Vector3.up, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);  // Front tires spin using velocity

                frontTireParents[i].transform.localEulerAngles = new Vector3(frontTireParents[i].transform.localEulerAngles.x, steeringAngle, frontTireParents[i].transform.localEulerAngles.z);
            }
            else  // Rear tires
            {
                tires[i].transform.Rotate(-Vector3.up, tireRotSpeed * ((carVelocityRatio + moveInput) / 2) * Time.deltaTime, Space.Self);  // Rear tires spin using velocity & acceleration
                //tires[i].transform.Rotate(-Vector3.up, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);  // Rear tires spin using velocity
                //tires[i].transform.Rotate(-Vector3.up, tireRotSpeed * moveInput * Time.deltaTime, Space.Self);  // Rear tires spin using acceleration
            }
        }
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition, int tireIndex)
    {
        if (isDrifting && driftDirection == 1 && tireIndex % 2 != 0) targetPosition.y = tire.transform.parent.position.y;
        if (isDrifting && driftDirection == -1 && tireIndex % 2 == 0) targetPosition.y = tire.transform.parent.position.y;

        tire.transform.position = Vector3.MoveTowards(tire.transform.position, targetPosition, tireSuspensionMoveSpeed);
    }

    private void VFX()
    {
        ToggleSkidMarks(false);


        for (int i = 0; i < skidMarks.Length; i++)
            skidMarks[i].transform.position = tires[i].transform.position;


        // Drifting
        if (isGrounded && isDrifting)
        {
            for (int i = 0; i < skidMarks.Length; i++)
            {
                if (driftDirection == 1 && i % 2 == 0) skidMarks[i].emitting = true;
                else if (driftDirection == -1 && i % 2 != 0) skidMarks[i].emitting = true;
            }
        }
        // Braking
        else if (isGrounded && isBraking)
        {
            ToggleSkidMarks(true);
        }
        // Turning
        else if (isGrounded && Mathf.Abs(currentCarLocalVelocity.x) > minSideSkidVelocity && carVelocityRatio > 0)
        {
            for (int i = 0; i < skidMarks.Length; i++)
            {
                if (i > 1) skidMarks[i].emitting = true;
            }
        }
    }

    private void ToggleSkidMarks(bool toggle) { foreach (var skidMark in skidMarks) { skidMark.emitting = toggle; } }
    #endregion



    #region References
    public float GetCarVelocityRatio() { return carVelocityRatio; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsBraking() { return isBraking; }
    public bool IsDrifting() { return isDrifting; }
    public bool IsBrakeDrifting() { return isDrifting && Input.GetButton("Fire1"); }
    public float GetSteerInput() { return steerInput; }
    public float GetDriftDirection() { return driftDirection; }
    public float GetDriftControl() { return driftControl; }

    #endregion
}


