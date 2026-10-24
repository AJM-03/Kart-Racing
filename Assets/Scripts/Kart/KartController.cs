using Cinemachine;
using UnityEngine;

public class KartController : MonoBehaviour
{
    public KartEnums.CurrentTerrainState currentTerrainState;
    public int playerNumber;
    public bool canMove, canTransform;

    [HideInInspector] public Rigidbody rb;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private KartWallInteractions wallInteractions;
    public KartInput inputSource;
    public KartInputData currentInput;
    private bool isBraking = false;  // Whether the player is braking or not


    [Header("Kart Forms")]
    public KartEnums.KartForm currentForm;
    public KartForm currentFormScript;
    public Car car;

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
        BrakeCheck();
        currentFormScript.CalculateKartVelocity();
        wallInteractions.WallDetection();
        currentFormScript.FixedUpdateKart();
    }

    public void ChangeForm(KartEnums.KartForm targetForm, bool instant)
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

    private void SetCenterOfMass()
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

    private void BrakeCheck() { isBraking = (currentInput.Accelerate < 0 && GetKartVelocityRatio() > 0 && currentTerrainState == KartEnums.CurrentTerrainState.Land); }

    public Vector3 GetKartVelocity() { return currentFormScript.currentCarLocalVelocity; }
    public float GetKartVelocityRatio() { return currentFormScript.carVelocityRatio; }
}
