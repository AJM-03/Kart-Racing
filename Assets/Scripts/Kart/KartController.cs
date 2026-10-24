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

    [Header("Kart Forms")]
    public KartEnums.KartForm currentForm;
    public KartForm currentFormScript;
    public Car car;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentForm = KartEnums.KartForm.Car;
        canMove = true;
    }


    void Update()
    {
        if (canMove) inputSource.GetInput();
    }

    public void Transform(KartEnums.KartForm targetForm)
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
}
