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

    [Header("Car Status")]
    private int[] groundedWheels = new int[4];  // How many wheels are currently touching the ground
    private bool isGrounded = false;  // Whether the car is on the ground (requires 1 wheel on the ground)
    private float airTime;  // How long the car has been in the air

    [Header("Input")]
    private float moveInput = 0;  // The player's current accel & decel input
    private float steerInput = 0;  // The player's current steering input

    [Header("Acceleration")]
    [SerializeField] private float acceleration = 25f;  // The acceleration of the car
    [SerializeField] private float maxSpeed = 100f;  // The top speed of the car
    [SerializeField] private float deceleration = 10f;  // How quickly the car will slow down
    [SerializeField] private float dragCoefficient = 1f;  // Side force preventing the car from sliding

    [Header("Braking")]
    [SerializeField] private float brakingDeceleration = 100f;  // How quickly you decelerate whilst braking
    [SerializeField] private float brakingDragCoefficient = 0.5f;  // Side force preventing the car from sliding whilst braking
    private bool isBraking = false;  // Whether the player is braking or not

    [Header("Reversing")]
    [SerializeField] private float reverseAcceleration = 15f;  // Acceleration whilst reversing
    [SerializeField] private float maxReverseSpeed = 20f;  // Top speed whilst reversing

    [Header("Steering")]
    [SerializeField] private float steerStrength = 15f;  // The car's handling
    [SerializeField] private AnimationCurve turningCurve;  // Dynamically change turning strength based on the car's velocity
    [SerializeField] private AnimationCurve steeringSwingOut;  // How far the car swings out based on velocity and steering strength

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

    [Header("Car Rotation")]
    [SerializeField] private float carRotationMaxX = 60f;  // The car's maximum X rotation
    [SerializeField] private float carRotationMaxZ = 60f;  // The car's maximum Z rotation
    [SerializeField] private AnimationCurve airRotationCurve;  // How quickly the car returns to defualt rotation while in the air

    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;  // How quickly tires will spin at maxSpeed
    [SerializeField] private float maxTireSteeringAngle = 30f;  // How far the front tires will rotate when steering
    [SerializeField] private float minSideSkidVelocity = 10f;  // How fast you need to be sliding for skid marks to appear on rear tires
    [SerializeField, Range(0f, 5f)] private float modelRotationSpeed = 0.1f;  // How quickly the car model will rotate
    [SerializeField, Range(0, 120)] private float maxDriftCarAngle = 30f;  // How far the car will turn when sharply brake drifting
    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[4];  // The four skid mark trail renderers
    [SerializeField, Range(0f, 5f)] private float cameraHeadingChange = 0.75f;  // How far the camera will move left and right when steering
    [SerializeField] private float tireSuspensionMoveSpeed = 0.01f;  // How quickly the tires move up and down with the terrain
    private Vector3[] tirePositions = new Vector3[4];  // The starting positions for each tire
    private Quaternion targetCarRotation;  // The rotation that the car wants to reach

    [Header("Debug")]
    [SerializeField] private bool wheelRays = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        virtualCamera = Camera.main.transform.parent.GetComponent<CinemachineVirtualCamera>();

        SetCenterOfMass();

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
        Suspension();
        GroundCheck();
        BrakeCheck();
        CalculateCarVelocity();
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
        currentCarLocalVelocity = transform.InverseTransformDirection(rb.velocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    private void SetCenterOfMass()
    {
        float x = 0;
        float z = 0;
        foreach(Transform point in rayPoints)
        {
            x += point.position.x;
            z += point.position.z;
        }
        rb.centerOfMass = new Vector3(x, -0.25f, z); // Lower the center of mass.
    }

    private void UpdateDebugStats()
    {
        DebugStats.moveInput = moveInput;
        if (isDrifting) DebugStats.steerInput = driftControl * driftDirection;
        else DebugStats.steerInput = steerInput;
        DebugStats.braking = isBraking;
        DebugStats.currentCarLocalVelocity = currentCarLocalVelocity;
        DebugStats.carVelocityRatio = carVelocityRatio;

        int gW = 0;
        for (int i = 0; i < groundedWheels.Length; i++) { gW += groundedWheels[i]; }
        DebugStats.groundedWheels = gW;

        if (wheelRays)
        {
            foreach (Transform p in rayPoints)
            {
                p.gameObject.GetComponent<DebugWheelForces>().SetForce();
            }
        }
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
        bool reversing = (moveInput < 0 && carVelocityRatio < 0);
        if (Mathf.Abs(currentCarLocalVelocity.z) >= (!reversing ? maxSpeed : maxReverseSpeed)) return;

        rb.AddForceAtPosition((!reversing ? acceleration : reverseAcceleration) * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        rb.AddForce((isBraking ? brakingDeceleration : deceleration) * carVelocityRatio * -rb.transform.forward, ForceMode.Acceleration);
    }

    private void Turn()
    {
        float turnAmount = steerInput;

        if (isDrifting) 
            turnAmount = driftControl * driftDirection;

        rb.AddRelativeTorque(steerStrength * turnAmount * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void Drift()
    {
        if (Input.GetButtonDown("Jump") && !isDrifting && steerInput != 0 && carVelocityRatio >= minVelocityRatioToDrift)
        {
            isDrifting = true;
            driftDirection = steerInput > 0 ? 1 : -1;
        }

        if ((Input.GetButtonUp("Jump") || carVelocityRatio < minVelocityRatioToDrift) && isDrifting)
        {
            isDrifting = false;
        }

        if (isDrifting)
        {
            if (Input.GetButton("Fire2")) // Brake Drifting
                driftControl = (driftDirection == 1) ? ExtensionMethods.Remap(steerInput, -1, 1, brakeDriftWideTurn, brakeDriftSharpTurn) : ExtensionMethods.Remap(steerInput, -1, 1, brakeDriftSharpTurn, brakeDriftWideTurn);
            else
                driftControl = (driftDirection == 1) ? ExtensionMethods.Remap(steerInput, -1, 1, driftWideTurn, driftSharpTurn) : ExtensionMethods.Remap(steerInput, -1, 1, driftSharpTurn, driftWideTurn);
        }
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * (isBraking ? brakingDragCoefficient : dragCoefficient);

        Vector3 dragForce = transform.right * dragMagnitude;

        rb.AddForceAtPosition(dragForce, rb.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void CarRotation()
    {





        // Get current rotation in Euler angles
        Vector3 currentRotation = transform.localEulerAngles;

        // Convert Unity's 0–360 range to -180–180 for easier clamping
        currentRotation.x = NormalizeAngle(currentRotation.x);
        currentRotation.z = NormalizeAngle(currentRotation.z);

        // Clamp each axis
        currentRotation.x = Mathf.Clamp(currentRotation.x, -carRotationMaxX, carRotationMaxX);
        currentRotation.z = Mathf.Clamp(currentRotation.z, -carRotationMaxZ, carRotationMaxZ);

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

        // Apply clamped rotation
        transform.localEulerAngles = currentRotation;
    }

    // Converts angles from 0–360 to -180–180
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    #endregion

    #region Suspension
    private void Suspension()
    {
        for(int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxDistance = restLength + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + wheelRadius, driveableLayer))
            {
                groundedWheels[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;  // How much the spring is compressed in a normalized format

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                // Visuals
                SetTirePosition(tires[i], hit.point + rayPoints[i].up * wheelRadius, i);

                if (wheelRays) rayPoints[i].GetComponent<DebugWheelForces>().SetSuspension(rayPoints[i].position + (netForce) * rayPoints[i].up );
            }
            else
            {
                groundedWheels[i] = 0;

                // Visuals
                SetTirePosition(tires[i], rayPoints[i].position - rayPoints[i].up * maxDistance, i);

                if (wheelRays) rayPoints[i].GetComponent<DebugWheelForces>().SetSuspension(rayPoints[i].position + (wheelRadius + maxDistance) * -rayPoints[i].up);
            }
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
        targetCarRotation = Quaternion.Euler(new Vector3(0, Mathf.Lerp(0, maxDriftCarAngle, y) * (isDrifting ? driftDirection : steerInput), Mathf.Lerp(0, maxDriftCarAngle, y) * (isDrifting ? driftDirection : steerInput) / 3)); 

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
                tires[i].transform.Rotate(-Vector3.up, tireRotSpeed * moveInput * Time.deltaTime, Space.Self);  // Rear tires spin using acceleration
            }
        }
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition, int tireIndex)
    {
        tire.transform.parent.localPosition = new Vector3(tirePositions[tireIndex].x, 0, tirePositions[tireIndex].z);
        float tireY = targetPosition.y;
        if (isDrifting && driftDirection == 1 && tireIndex % 2 != 0) tireY = tire.transform.parent.position.y - restLength;
        if (isDrifting && driftDirection == -1 && tireIndex % 2 == 0) tireY = tire.transform.parent.position.y - restLength;

        tire.transform.position = Vector3.MoveTowards(tire.transform.position, new Vector3(tire.transform.parent.position.x, tireY, tire.transform.parent.position.z), tireSuspensionMoveSpeed);
        //tire.transform.position = new Vector3(tire.transform.parent.position.x, tireY, tire.transform.parent.position.z);
    }

    private void VFX()
    {
        ToggleSkidMarks(false);

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
        if (orbitalTransposer != null) orbitalTransposer.m_Heading.m_Bias = turnAmount * carVelocityRatio * cameraHeadingChange;
    }
    #endregion
}


