using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACarController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform[] rayPoints;  // Where the wheel rays come from
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private Transform carModel;
    [SerializeField] private GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private LayerMask driveableLayer;  // Layers that the car is able to drive on

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;  // The max force a spring can exhert when fully compressed
    [SerializeField] private float damperStiffness;  // Calculate using https://youtu.be/sWshRRDxdSU?t=569
    [SerializeField] private float restLength;  // Standard length of a spring when not being compressed or stretched
    [SerializeField] private float springTravel;  // Max distance a spring can compress or extend from it's rest position
    [SerializeField] private float wheelRadius;  // The size of the wheel from center to bottom
    [SerializeField] private float driveableSlopeLimt;  // How steep a slope has to be to not be considered driveable

    [Header("Car Status")]
    private int playerNumber = 0;
    private int[] groundedWheels = new int[4];  // How many wheels are currently touching the ground
    private bool isGrounded = false;  // Whether the car is on the ground (requires 1 wheel on the ground)
    private float airTime;  // How long the car has been in the air
    [HideInInspector] public bool canMove;  // Whether the car can move

    [Header("Input")]
    private float moveInput = 0;  // The player's current accel & decel input
    private float steerInput = 0;  // The player's current steering input

    [Header("Acceleration")]
    [SerializeField] private float acceleration = 25f;  // The acceleration of the car
    [SerializeField] private float maxSpeed = 100f;  // The top speed of the car
    [SerializeField] private AnimationCurve accelerationCurve;  // The curve of how your car will reach top speed, using carVelocityRatio
    [SerializeField] private float deceleration = 10f;  // How quickly the car will slow down
    [SerializeField] private float dragCoefficient = 1f;  // Side force preventing the car from sliding
    private bool isReversing;  // Whether the car is reversing or not

    [Header("Braking")]
    [SerializeField] private float brakingDeceleration = 100f;  // How quickly you decelerate whilst braking
    [SerializeField] private float brakingDragCoefficient = 0.5f;  // Side force preventing the car from sliding whilst braking
    private bool isBraking = false;  // Whether the player is braking or not

    [Header("Reversing")]
    [SerializeField] private float reverseAcceleration = 15f;  // Acceleration whilst reversing
    [SerializeField] private float maxReverseSpeed = 20f;  // Top speed whilst reversing
    [SerializeField] private AnimationCurve reverseAccelerationCurve;  // The curve of how your car will reach top reverse speed, using carVelocityRatio


    [Header("Steering")]
    [SerializeField] private float steerStrength = 15f;  // The car's handling
    [SerializeField] private AnimationCurve turningCurve;  // Dynamically change turning strength based on the car's velocity
    private float steeringTime;  // How long the car has been steering in this direction
    private int steeringDirection;  // Which way the car is steering (-1 = Left, 1 = Right)

    [SerializeField] private float steerSwingStrength = 5;  // How much the car will swing out when turning
    [SerializeField] private AnimationCurve steeringSwingCurve;  // How far the car swings out based on velocity and steering strength
    [SerializeField] private float steerSwingTime = 1;  // How long the swing will last
    [SerializeField] private AnimationCurve steeringSwingTimeCurve;  // The strength of the swing over time


    [Header("Drifting")]
    [SerializeField, Range(0f, 1f)] private float minVelocityRatioToDrift = 0.2f;  // How fast you must be going in comparison to top speed in order to drift
    [SerializeField, Range(1f, 3f)] private float driftSharpTurn = 2;  // How wide you can drift
    [SerializeField, Range(0, 1.5f)] private float driftWideTurn = 0;  // How tight you can drift
    [SerializeField, Range(1f, 3f)] private float brakeDriftSharpTurn = 2;  // How wide you can brake drift
    [SerializeField, Range(0, 1.5f)] private float brakeDriftWideTurn = 0;  // How tight you can brake drift
    private bool isDrifting = false;  // Whether the car is drifting or not
    private int driftDirection;  // Which way the car is drifting (-1 = Left, 1 = Right)
    private float driftControl;  // How tightly you are drifting

    private Vector3 currentCarLocalVelocity = Vector3.zero;  // The current speed of the car
    private float carVelocityRatio = 0;  // Current speed in comparison to top speed

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallDetectionLayer;  // The layers that count as walls
    [SerializeField] private float wallDetectionDistance = 3;  // How far away the car will detect walls
    [SerializeField] private float wallCollisionRedirectSpeed;  // How fast driving into a wall will turn you
    private bool carCollidingWithWall;  // If the car is currently touching a wall

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
    [SerializeField, Range(0f, 5f)] private float cameraHeadingChange = 0.75f;  // How far the camera will move left and right when steering
    [SerializeField, Range(0f, 5f)] private float driftCameraHeadingChange = 0.75f;  // How far the camera will move left and right when drifting
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
        CameraMovement();
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
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < groundedWheels.Length; i++)
        {
            tempGroundedWheels += groundedWheels[i];  // If wheel is grounded add one
        }

        if (tempGroundedWheels > 1)
        {
            isGrounded = true;
            airTime = 0;
        }
        else
        {
            isGrounded = false;
            airTime += Time.deltaTime;
        }
    }

    private void BrakeCheck() { isBraking = (moveInput < 0 && carVelocityRatio > 0 && isGrounded); }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(rb.velocity).Round(2);

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
        foreach(Transform point in rayPoints)
        {
            x += point.localPosition.x;
            z += point.localPosition.z;
        }
        rb.centerOfMass = new Vector3(x, -0.25f, z); // Lower the center of mass.
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision == null) return;
        if (wallDetectionLayer.Contains(collision.gameObject.layer))
        {
            Debug.Log("hit " + collision.gameObject.name);
        }
    }

    private void WallDetection()
    {
        Transform lRayPoint = rayPoints[0];
        Transform rRayPoint = rayPoints[1];

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

            if (lDidHit) lDist = lHit.distance;  // Third ray will count as the left ray hitting
            if (lDist == Mathf.Infinity) return;
        }

        if (lDidHit)
        {
            Debug.Log(Vector3.Angle(lRayPoint.forward, lHit.normal) / 180);
        }

        bool lCloser = lDist == Mathf.Min(lDist, rDist);
        float closerDist = lCloser ? lDist : rDist;
        float turnAmount = (closerDist / wallDetectionDistance);
        if (!lCloser) turnAmount *= -1;

        float carSpeed = Mathf.Max(carVelocityRatio, moveInput);
        float hitAngle = (Vector3.Angle(lCloser ? lRayPoint.forward : rRayPoint.forward, lCloser ? lHit.normal : rHit.normal) - 90) / 90;  // 1 for head on, 0 for parallel

        rb.AddRelativeTorque(steerStrength * turnAmount * hitAngle * transform.up, ForceMode.Acceleration);
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

    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = Input.GetButton("Fire1") ? 1 : (Input.GetButton("Fire2") ? -1 : 0);
        steerInput = Input.GetAxis("Horizontal");
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
            if (Input.GetButton("Fire2")) // Brake Drifting
                driftControl = (driftDirection == 1) ? ExtensionMethods.Remap(steerInput, -1, 1, brakeDriftWideTurn, brakeDriftSharpTurn) : ExtensionMethods.Remap(steerInput, -1, 1, brakeDriftSharpTurn, brakeDriftWideTurn);
            else  // Standard Drifting
                driftControl = (driftDirection == 1) ? ExtensionMethods.Remap(steerInput, -1, 1, driftWideTurn, driftSharpTurn) : ExtensionMethods.Remap(steerInput, -1, 1, driftSharpTurn, driftWideTurn);
        }
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * (isBraking ? brakingDragCoefficient : dragCoefficient);

        Vector3 dragForce = transform.right * dragMagnitude;


        // Steering Swing
        if (steerSwingStrength != 0)
        {
            float swingAmount = steerSwingStrength;
            if (!isDrifting) swingAmount *= steeringSwingCurve.Evaluate(Mathf.Abs(carVelocityRatio) * Mathf.Abs(steerInput)) * steeringSwingTimeCurve.Evaluate(steeringTime / steerSwingTime);  // Calculate how much to swing out by when not drifting
            else             swingAmount *= steeringSwingCurve.Evaluate(Mathf.Abs(carVelocityRatio));

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

        // Convert Unity's 0–360 range to -180–180 for easier clamping
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
    private void Suspension()
    {
        for(int i = 0; i < rayPoints.Length; i++)
        {
            // Wheel to Ground Raycast
            RaycastHit hit;
            bool didHit;
            float maxDistance = restLength + springTravel;
            didHit = Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + wheelRadius, driveableLayer);

            // Visual Wheel to Ground Raycast
            RaycastHit visualHit;
            Physics.Raycast(tires[i].transform.parent.position, -tires[i].transform.parent.up, out visualHit, maxDistance + wheelRadius, driveableLayer);

            // Surface Slope
            Vector3 surfaceNormal = hit.normal;  // Get the surface normal
            float slopeAngle = Vector3.Angle(surfaceNormal, Vector3.up);  // Calculate slope angle relative to world up
            Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, surfaceNormal), surfaceNormal).normalized; // Get slope direction(downhill vector)
            Debug.Log($"Slope Angle: {slopeAngle:F2}° | Slope Direction: {slopeDirection}");


            if (didHit && slopeAngle <= driveableSlopeLimt)  // If the wheel is on something it can drive on
            {              
                groundedWheels[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;  // How much the spring is compressed in a normalized format

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);
            }
            else
            {
                groundedWheels[i] = 0;
            }

            // Visuals
            if (visualHit.collider != null) SetTirePosition(tires[i], visualHit.point + rayPoints[i].up * wheelRadius, i);
            else SetTirePosition(tires[i], tires[i].transform.parent.position, i);
        }
    }
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
        if (isDrifting && !isBraking) targetTilt /= 2f;
        if (!isDrifting) targetTilt /= 2.5f;

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

    private void CameraMovement()
    {
        var orbitalTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        float turnAmount = steerInput;
        if (isDrifting) turnAmount = driftControl * driftDirection;
        if (!isGrounded) turnAmount = 0;
        if (orbitalTransposer != null) orbitalTransposer.m_Heading.m_Bias = turnAmount * carVelocityRatio * (isDrifting ? driftCameraHeadingChange : cameraHeadingChange);
    }
    #endregion
}


