using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACarController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private Transform accelerationPoint;
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

    [Header("Input")]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;  // Dynamically change turning strength based on the car's velocity
    [SerializeField] private float dragCoefficient = 1f;  // Side force preventing the car from sliding

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;  // Current speed in comparison to top speed

    [Header("Debug")]
    [SerializeField] private bool wheelRays = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
    }

    #region Car Status Check
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

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(rb.velocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }
    #endregion

    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
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
        rb.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        rb.AddForceAtPosition(deceleration * moveInput * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        rb.AddTorque(steerStrength * steerInput * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * dragCoefficient;

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
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, driveableLayer))
            {
                groundedWheels[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;  // How much the spring is compressed in a normalized format

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                if (wheelRays) Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                groundedWheels[i] = 0;

                if (wheelRays) Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
            }
        }
    }
    #endregion
}
