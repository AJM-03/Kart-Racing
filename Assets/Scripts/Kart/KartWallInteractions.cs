using UnityEngine;

public class KartWallInteractions : MonoBehaviour
{
    private KartController kart;
    private Transform[] rayPoints;  // Where the rays come from, taken from the car wheels

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallDetectionLayer;  // The layers that count as walls
    [SerializeField] private float wallDetectionDistance = 1f;  // How far away the car will detect walls
    [SerializeField] private float wallCollisionRedirectSpeed = 22f;  // How fast driving into a wall will turn you
    [SerializeField] private AnimationCurve wallBouncebackAngle;  // X-Angle you hit the wall (0-Paralell, 1-Head On), Y-Bounceback
    [SerializeField] private float wallBouncebackPower = 20000f;  // How much hitting the wall head on will bounce the player backwards
    private float wallAngle;  // The angle at which the car is facing a wall if there is one


    void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
        rayPoints = kart.car.wheels.rayPoints;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        if (wallDetectionLayer.Contains(collision.gameObject.layer))  // When driving into a wall
        {
            WallBounceback(collision);
        }
    }

    public void WallDetection()
    {
        wallAngle = 0;

        float carSpeed = Mathf.Max(Mathf.Abs(kart.GetVelocityRatio()), Mathf.Abs(kart.currentInput.Accelerate));
        if (carSpeed <= 0.1f) return;  // If the car is not actually moving then don't redirect

        Transform lRayPoint, rRayPoint;
        if (kart.GetVelocityRatio() >= 0.1f || kart.currentInput.Accelerate > 0)  // If the car is driving forwards
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

        kart.rb.AddRelativeTorque(wallCollisionRedirectSpeed * turnAmount * wallAngle * transform.up, ForceMode.Acceleration);  // Turn the car to be parallel with the wall
    }

    private void WallBounceback(Collision collision)  // When driving into a wall head-on
    {
        float bounceback = wallBouncebackAngle.Evaluate(wallAngle);

        if (collision.gameObject.TryGetComponent<BounceKartBack>(out BounceKartBack bounce)) bounceback = bounce.bouncePower;

        if (kart.GetVelocityRatio() > 0 && bounceback > 0.1f)
        {
            Debug.Log("hit " + collision.gameObject.name + "  -  Hit angle-" + bounceback + "  -  Bounceback Force-" + bounceback * wallBouncebackPower * kart.GetVelocityRatio());
            kart.rb.AddForce(bounceback * wallBouncebackPower * kart.GetVelocityRatio() * -kart.rb.transform.forward, ForceMode.Impulse);
        }
    }
}
