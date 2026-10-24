using UnityEngine;


public class Wheels : MonoBehaviour
{
    [Header("References")]
    public Transform[] rayPoints;  // Where the wheel rays come from
    public GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];
    private KartController kart;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness = 35000f;  // The max force a spring can exhert when fully compressed
    [SerializeField] private float damperStiffness = 3500f;  // Calculate using https://youtu.be/sWshRRDxdSU?t=569
    [SerializeField] private float restLength = 0.375f;  // Standard length of a spring when not being compressed or stretched
    [SerializeField] private float springTravel = 0.2f;  // Max distance a spring can compress or extend from it's rest position
    [SerializeField] private float wheelRadius = 0.45f;  // The size of the wheel from center to bottom
    private int[] groundedWheels = new int[4];  // How many wheels are currently touching the ground

    [Header("Surface Settings")]
    [SerializeField] private LayerMask driveableLayer;  // Layers that the car is able to drive on
    [SerializeField] private float driveableSlopeLimt = 46f;  // How steep a slope has to be to not be considered driveable



    void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);


    }



    public void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
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
            if (slopeAngle != 0) Debug.Log($"Slope Angle: {slopeAngle:F2}� | Slope Direction: {slopeDirection}");


            if (didHit && slopeAngle <= driveableSlopeLimt)  // If the wheel is on something it can drive on
            {
                groundedWheels[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;  // How much the spring is compressed in a normalized format

                float springVelocity = Vector3.Dot(kart.rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                kart.rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);
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


    public bool GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < groundedWheels.Length; i++)
        {
            tempGroundedWheels += groundedWheels[i];  // If wheel is grounded add one
        }

        return tempGroundedWheels > 1;
    }
}
