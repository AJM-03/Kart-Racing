using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    private KartController kart;
    private Car car;
    private Wheels wheels;

    [Header("Car Rotation")]
    private Quaternion targetCarRotation;  // The rotation that the car wants to reach
    [SerializeField] private float carRotationMaxX = 60f;  // The car's maximum X rotation
    [SerializeField] private float carRotationMaxZ = 60f;  // The car's maximum Z rotation
    [SerializeField] private AnimationCurve airRotationCurve;  // How quickly the car returns to defualt rotation while in the air
    [SerializeField, Range(0, 160)] private float maxDriftModelTurnAngle = 30f;  // How far the car will turn when sharply brake drifting
    [SerializeField, Range(0, 160)] private float maxDriftModelTiltAngle = 30f;  // How far the car will turn when sharply brake drifting


    [Header("Wheels")]
    [SerializeField] private float tireRotSpeed = 3000f;  // How quickly tires will spin at maxSpeed
    [SerializeField] private float maxTireSteeringAngle = 30f;  // How far the front tires will rotate when steering
    [SerializeField] private float minSideSkidVelocity = 10f;  // How fast you need to be sliding for skid marks to appear on rear tires
    [SerializeField, Range(0f, 5f)] private float modelRotationSpeed = 0.1f;  // How quickly the car model will rotate
    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[4];  // The four skid mark trail renderers
    [SerializeField] private float tireSuspensionMoveSpeed = 0.01f;  // How quickly the tires move up and down with the terrain
    private Vector3[] tirePositions = new Vector3[4];  // The starting positions for each tire


    void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart) car = kart.car;
        if (car) wheels = car.wheels;
        if (wheels == null) Debug.LogError("Kart references not found by " + this);

        for (int i = 0; i < wheels.tires.Length; i++) tirePositions[i] = wheels.tires[i].transform.parent.localPosition;
    }

    public void SetVisuals()
    {
        CarRotation();
        TireVisuals();
        VFX();
        TurnCarRotation();
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
        if (kart.currentTerrainState == KartEnums.CurrentTerrainState.Sky)
        {
            if (currentRotation.x < -1)
                currentRotation.x += airRotationCurve.Evaluate(car.airTime);
            else if (currentRotation.x > 1)
                currentRotation.x -= airRotationCurve.Evaluate(car.airTime);

            if (currentRotation.z < -1)
                currentRotation.z += airRotationCurve.Evaluate(car.airTime);
            else if (currentRotation.z > 1)
                currentRotation.z -= airRotationCurve.Evaluate(car.airTime);
        }

        transform.localEulerAngles = currentRotation;
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
}
