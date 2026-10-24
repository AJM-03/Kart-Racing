using UnityEngine;

public class Car : KartForm
{
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private Transform carModel;
    public Wheels wheels;
    private KartController kart;


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
    [SerializeField, Range(1f, 2.5f)] private float driftSharpTurn = 2;  // How wide you can drift
    [SerializeField, Range(0, 1.5f)] private float driftWideTurn = 0;  // How tight you can drift
    [SerializeField, Range(1.5f, 3f)] private float brakeDriftSharpTurn = 2;  // How wide you can brake drift
    [SerializeField, Range(0, 1.5f)] private float brakeDriftWideTurn = 0;  // How tight you can brake drift
    [SerializeField] private AnimationCurve driftControlCurve;  // How tight you turn with input, from sharp to wide
    [SerializeField] private AnimationCurve driftSwingCurve;  // The sideways force that will be applied to the car during a drift
    private bool isDrifting = false;  // Whether the car is drifting or not
    private int driftDirection;  // Which way the car is drifting (-1 = Left, 1 = Right)
    private float driftControl;  // How tightly you are drifting



    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallDetectionLayer;  // The layers that count as walls
    [SerializeField] private float wallDetectionDistance = 3;  // How far away the car will detect walls
    [SerializeField] private float wallCollisionRedirectSpeed;  // How fast driving into a wall will turn you
    [SerializeField] private AnimationCurve wallBouncebackAngle;  // X-Angle you hit the wall (0-Paralell, 1-Head On), Y-Bounceback
    [SerializeField] private float wallBouncebackPower;  // How much hitting the wall head on will bounce the player backwards
    private float wallAngle;  // The angle at which the car is facing a wall if there is one

    private float airTime;  // How long the car has been in the air


    void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
    }



    public override void UpdateKart()
    {
        DriftInput();
    }


    public override void FixedUpdateKart()
    {
        wheels.Suspension();
        GroundCheck();
    }



    void GroundCheck()
    {
        bool grounded = wheels.GroundCheck();
        if (grounded) airTime = 0;
        else airTime += Time.deltaTime;
    }

    public override void CalculateKartVelocity()
    {
        kart.currentCarLocalVelocity = transform.InverseTransformDirection(kart.rb.linearVelocity).Round(2);

        if (kart.currentCarLocalVelocity.z >= 0) kart.carVelocityRatio = kart.currentCarLocalVelocity.z / maxSpeed;
        else kart.carVelocityRatio = kart.currentCarLocalVelocity.z / maxReverseSpeed;

        kart.carVelocityRatio = kart.carVelocityRatio.Round(2);
        if (kart.carVelocityRatio >= 0.975) kart.carVelocityRatio = 1;
        if (kart.carVelocityRatio <= -0.975) kart.carVelocityRatio = -1;
    }


    private void DriftInput()
    {
        float steerInput = kart.currentInput.Steer;

        if (Input.GetButtonDown("Jump") && !isDrifting && steerInput != 0 && kart.carVelocityRatio >= minVelocityRatioToDrift)  // Start Drifting
        {
            isDrifting = true;
            driftDirection = steerInput > 0 ? 1 : -1;
        }

        if ((Input.GetButtonUp("Jump") || kart.carVelocityRatio < minVelocityRatioToDrift) && isDrifting)  // Stop Drifting
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



    public override void TransformInto(KartEnums.KartForm fromForm)
    {
        throw new System.NotImplementedException();
    }

    public override void TransformOut(KartEnums.KartForm toForm)
    {
        throw new System.NotImplementedException();
    }
}
