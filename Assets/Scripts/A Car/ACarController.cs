using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACarController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private Transform carModel;
    [SerializeField] private GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private LayerMask driveableLayer;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;  // The max force a spring can exhert when fully compressed
    [SerializeField] private float damperStiffness;  // Calculate using https://youtu.be/sWshRRDxdSU?t=569
    [SerializeField] private float restLength;  // Standard length of a spring when not being compressed or stretched
    [SerializeField] private float springTravel;  // Max distance a spring can compress or extend from it's rest position
    [SerializeField] private float wheelRadius;

    [Header("Car Status")]
    private int[] groundedWheels = new int[4];
    private bool isGrounded = false;
    private bool isBraking = false;
    private bool isDrifting = false;
    private int driftDirection;
    private float driftControl;

    [Header("Input")]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float reverseAcceleration = 15f;
    [SerializeField] private float maxReverseSpeed = 20f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;  // Dynamically change turning strength based on the car's velocity
    [SerializeField] private float dragCoefficient = 1f;  // Side force preventing the car from sliding
    [SerializeField] private float brakingDeceleration = 100f;
    [SerializeField] private float brakingDragCoefficient = 0.5f;
    [SerializeField, Range(1f, 3f)] private float driftSharpTurn = 2;
    [SerializeField, Range(0, 1.5f)] private float driftWideTurn = 0;
    [SerializeField, Range(1f, 3f)] private float brakeDriftSharpTurn = 2;
    [SerializeField, Range(0, 1.5f)] private float brakeDriftWideTurn = 0;

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;  // Current speed in comparison to top speed

    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;
    [SerializeField] private float maxTireSteeringAngle = 30f;
    [SerializeField] private float minSideSkidVelocity = 10f;
    [SerializeField, Range(0, 120)] private float maxDriftCarAngle = 30f;
    private Vector3[] tirePositions = new Vector3[4];
    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[4];

    [Header("Debug")]
    [SerializeField] private bool wheelRays = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        SetCenterOfMass();

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
        Suspension();
        GroundCheck();
        BrakeCheck();
        CalculateCarVelocity();
        Movement();
        Visuals();
    }

    #region Car Status
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for(int i = 0; i < groundedWheels.Length; i++)
        {
            tempGroundedWheels += groundedWheels[i];  // If wheel is grounded add one
        }

        if (tempGroundedWheels > 1)
            isGrounded = true;
        else
            isGrounded = false;
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
        if (Input.GetButtonDown("Jump") && !isDrifting && steerInput != 0)
        {
            isDrifting = true;
            driftDirection = steerInput > 0 ? 1 : -1;
        }

        if (Input.GetButtonUp("Jump") && isDrifting)
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

                if (wheelRays) Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                groundedWheels[i] = 0;

                // Visuals
                SetTirePosition(tires[i], rayPoints[i].position - rayPoints[i].up * maxDistance, i);

                if (wheelRays) Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxDistance) * -rayPoints[i].up, Color.green);
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

        var orbitalTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        if (orbitalTransposer != null) orbitalTransposer.m_Heading.m_Bias = steerInput * 1;
    }

    private void TurnCarRotation()
    {
        if (!isGrounded) return;

        float turnAmount = steerInput;
        if (isDrifting) turnAmount = driftControl * driftDirection;
        float y = Mathf.InverseLerp(0, brakeDriftSharpTurn, Mathf.Abs(turnAmount));
        y = (y / 100) * (carVelocityRatio * 100);
        carModel.localEulerAngles = new Vector3(0, Mathf.Lerp(0, maxDriftCarAngle, y) * (isDrifting ? driftDirection : steerInput), 0); 
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
        tire.transform.position = new Vector3(tire.transform.parent.position.x, targetPosition.y, tire.transform.parent.position.z);
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
    #endregion
}


