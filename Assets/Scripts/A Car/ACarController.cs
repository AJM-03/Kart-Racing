using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACarController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask driveableLayer;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;  // The max force a spring can exhert when fully compressed
    [SerializeField] private float damperStiffness;  // Calculate using https://youtu.be/sWshRRDxdSU?t=569
    [SerializeField] private float restLength;  // Standard length of a spring when not being compressed or stretched
    [SerializeField] private float springTravel;  // Max distance a spring can compress or extend from it's rest position
    [SerializeField] private float wheelRadius;

    [Header("Debug")]
    [SerializeField] private bool wheelRays = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Suspension();
    }

    private void Suspension()
    {
        foreach(Transform rayPoint in rayPoints)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, driveableLayer))
            {
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;  // How much the spring is compressed in a normalized format

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoint.position), rayPoint.up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoint.up, rayPoint.position);

                if (wheelRays) Debug.DrawLine(rayPoint.position, hit.point, Color.red);
            }
            else
            {
                if (wheelRays) Debug.DrawLine(rayPoint.position,rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up, Color.green);
            }
        }
    }
}
