using Cinemachine;
using UnityEngine;

public class KartController : MonoBehaviour
{
    public KartEnums.CurrentTerrainState currentTerrainState;
    public int playerNumber;
    public bool canMove;

    [HideInInspector] public Rigidbody rb;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    public KartInput inputSource;
    public KartInputData currentInput;
    [HideInInspector] public Vector3 currentCarLocalVelocity = Vector3.zero;  // The current speed of the car
    [HideInInspector] public float carVelocityRatio = 0;  // Current speed in comparison to top speed

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


    void Update()
    {
        currentInput = inputSource.GetInput();
        currentFormScript.UpdateKart();
    }

    public void ChangeForm(KartEnums.KartForm targetForm, bool instant)
    {
        if (targetForm == currentForm) return;
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
        foreach (Transform point in rayPoints)
        {
            x += point.localPosition.x;
            z += point.localPosition.z;
        }
        rb.centerOfMass = new Vector3(x, -0.25f, z); // Lower the center of mass.
    }
}
