using Cinemachine;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [Header("Kart Info")]
    public int racerIndex;  // A unique ID for each racer in the game
    public int playerNumber;  // Represents what is controlling this racer 1-4=Local Player, 5=AI, 6=Network Player
    public bool canMove, canTransform;  // Can be used to limit the player's abilities
    public KartEnums.CurrentTerrainState currentTerrainState;  // The type of terrain the vehicle is currently on (Land, Sea, Sky)
    public KartInputData currentInput;  // The current input values for this frame


    [Header("References")]
    [HideInInspector] public Rigidbody rb;  // The kart's rigidBody component
    [SerializeField] private CinemachineVirtualCamera virtualCamera;  // The main camera attached to the kart
    [SerializeField] private KartWallInteractions wallInteractions;  // Detects and controls the kart's interactions with walls
    [SerializeField] private KartInput inputSource;  // The script that the input should be coming from


    [Header("Kart Forms")]
    public KartEnums.KartForm currentForm;  // The form that the kart is currently in
    public KartForm currentFormScript;  // The script for the kart's current form
    public Car car;  // The script for the car form



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        virtualCamera = Camera.main.transform.parent.GetComponent<CinemachineVirtualCamera>();
        ChangeForm(KartEnums.KartForm.Car, true);
        currentForm = KartEnums.KartForm.Car;
        canMove = true;

        SetCenterOfMass();
    }


    private void Start()
    {
        if (playerNumber > 1) Destroy(virtualCamera.transform.GetChild(0).gameObject);
    }



    private void Update()
    {
        currentInput = inputSource.GetInput();
        currentFormScript.UpdateKart();
    }



    private void FixedUpdate()
    {
        currentFormScript.CalculateKartVelocity();
        wallInteractions.WallDetection();
        currentFormScript.FixedUpdateKart();
    }



    public void ChangeForm(KartEnums.KartForm targetForm, bool instant)  // Changes the current form of the kart
    {
        if (targetForm == currentForm) return;
        if (!canTransform) return;

        currentFormScript.TransformOut(targetForm);
        KartEnums.KartForm oldForm = currentForm;

        currentForm = targetForm;
        if (targetForm == KartEnums.KartForm.Car) currentFormScript = car;
        if (targetForm == KartEnums.KartForm.Boat) currentFormScript = car;
        if (targetForm == KartEnums.KartForm.Plane) currentFormScript = car;

        currentFormScript.TransformInto(oldForm);
    }



    private void SetCenterOfMass()  // Initializes the kart's rigidbody by setting the centre of mass
    {
        float x = 0;
        float z = 0;
        foreach (Transform point in car.wheels.rayPoints)
        {
            x += point.localPosition.x;
            z += point.localPosition.z;
        }
        rb.centerOfMass = new Vector3(x, -0.25f, z); // Lower the center of mass.
    }



    // Getters
    public float GetVelocityRatio() { return currentFormScript.carVelocityRatio; }
    public Vector3 GetVelocity() { return currentFormScript.currentCarLocalVelocity; }
    public bool IsGrounded() { return currentTerrainState == KartEnums.CurrentTerrainState.Land; }
    public bool IsInWater() { return currentTerrainState == KartEnums.CurrentTerrainState.Sea; }
    public bool IsBraking() { return currentInput.Accelerate < 0 && GetVelocityRatio() > 0; }
    public bool IsDrifting() { return currentFormScript.isDrifting; }
    public bool IsBrakeDrifting() { return currentFormScript.isDrifting && IsBraking(); }
    public float GetDriftDirection() { return currentFormScript.driftDirection; }
    public float GetDriftControl() { return currentFormScript.driftControl; }
}
