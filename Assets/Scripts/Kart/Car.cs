using UnityEngine;

public class Car : KartForm
{
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private Transform carModel;
    public Wheels wheels;
    public CarVisuals carVisuals;
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





    [HideInInspector] public float airTime;  // How long the car has been in the air


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
        BrakeCheck();
        WallDetection();
        Movement();
        carVisuals.SetVisuals();
    }



    void GroundCheck()
    {
        bool grounded = wheels.GroundCheck();
        if (grounded) airTime = 0;
        else airTime += Time.deltaTime;
    }

    public override void CalculateKartVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(kart.rb.linearVelocity).Round(2);

        if (currentCarLocalVelocity.z >= 0) carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
        else carVelocityRatio = currentCarLocalVelocity.z / maxReverseSpeed;

        carVelocityRatio = carVelocityRatio.Round(2);
        if (carVelocityRatio >= 0.975) carVelocityRatio = 1;
        if (carVelocityRatio <= -0.975) carVelocityRatio = -1;
    }



    private void Movement()
    {
        if (kart.currentTerrainState == KartEnums.CurrentTerrainState.Land)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        }
    }

    private void Acceleration()
    {
        isReversing = (kart.currentInput.Accelerate < 0 && carVelocityRatio < 0);
        if (Mathf.Abs(currentCarLocalVelocity.z) >= (!isReversing ? maxSpeed : maxReverseSpeed)) return;  // If we are already at our max speed then stop


        Vector3 accelDirection = accelerationPoint.transform.forward;


        float accel = acceleration * accelerationCurve.Evaluate(carVelocityRatio);
        if (isReversing) accel = reverseAcceleration * reverseAccelerationCurve.Evaluate(carVelocityRatio);  // Reverse acceleration
        accel *= kart.currentInput.Accelerate;  // Multiply by the player's input (1 fowards, -1 reverse)

        kart.rb.AddForceAtPosition(accel * accelDirection, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Deceleration()
    {
        bool isBraking = kart.isBraking && kart.currentTerrainState == KartEnums.CurrentTerrainState.Land;
        kart.rb.AddForce((isBraking ? brakingDeceleration : deceleration) * carVelocityRatio * -kart.rb.transform.forward, ForceMode.Acceleration);
    }

    private void Turn()
    {
        float turnAmount = kart.currentInput.Steer;

        int newSteeringDirection = kart.currentInput.Steer > 0 ? 1 : -1;
        if (turnAmount == 0 || steeringDirection != newSteeringDirection) steeringTime = 0;
        else steeringTime += Time.fixedDeltaTime;
        steeringDirection = newSteeringDirection;
        //Debug.Log("Steering Time - " + steeringTime);
        if (isDrifting)
            turnAmount = driftControl * driftDirection;

        kart.rb.AddRelativeTorque(steerStrength * turnAmount * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }



    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * (isBraking ? brakingDragCoefficient : dragCoefficient);

        Vector3 dragForce = transform.right * dragMagnitude;


        // Steering Swing
        if (steerSwingStrength != 0 && kart.currentTerrainState == KartEnums.CurrentTerrainState.Land)
        {
            float swingAmount = steerSwingStrength;
            if (!isDrifting)
            {
                swingAmount *= steeringSwingCurve.Evaluate(Mathf.Abs(carVelocityRatio) * Mathf.Abs(kart.currentInput.Steer)) * steeringSwingTimeCurve.Evaluate(steeringTime / steerSwingTime);  // Calculate how much to swing out by when not drifting
            }
            else
            {
                swingAmount *= driftSwingCurve.Evaluate(driftControl);
            }

            if (isReversing) swingAmount = 0;  // Don't swing if reversing
            if ((!isDrifting && kart.currentInput.Steer > 0) || (isDrifting && driftDirection > 0)) swingAmount *= -1;  // Flip the force if steering right

            Vector3 swingForce = transform.right * swingAmount;
            dragForce += swingForce;
        }


        kart.rb.AddForceAtPosition(dragForce, kart.rb.worldCenterOfMass, ForceMode.Acceleration);
    }


    private void DriftInput()
    {
        float steerInput = kart.currentInput.Steer;

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



    public override void TransformInto(KartEnums.KartForm fromForm)
    {
        throw new System.NotImplementedException();
    }

    public override void TransformOut(KartEnums.KartForm toForm)
    {
        throw new System.NotImplementedException();
    }
}
