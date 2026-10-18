using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public ACarController car;
    private CinemachineVirtualCamera cam;
    private CinemachineTransposer transposer;


    [Header("Damping")]
    public float offsetDampingSpeed;

    [Header("Starting & Target Values")]
    private Vector3 stationaryCamOffset;
    private Vector3 targetOffset;
    private float stationaryCamFOV;
    private float targetFOV;

    [Header("Speed")]
    public AnimationCurve speedOffsetCurve;
    public Vector3 topSpeedOffsetChange;
    public float topSpeedFOVChange;

    [Header("Braking")]
    public AnimationCurve brakingOffsetCurve;
    public Vector3 brakingOffsetChange;
    public float brakingFOVChange;


    void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        transposer = cam.GetCinemachineComponent<CinemachineTransposer>();

        stationaryCamOffset = transposer.m_FollowOffset;
        stationaryCamFOV = cam.m_Lens.FieldOfView;
    }

    void Update()
    {
        targetOffset = stationaryCamOffset;
        targetFOV = stationaryCamFOV;

        if (car.isBraking)  // If braking
        {
            targetOffset += (brakingOffsetChange * brakingOffsetCurve.Evaluate(car.GetCarVelocityRatio()));
            targetFOV += (brakingFOVChange * brakingOffsetCurve.Evaluate(car.GetCarVelocityRatio()));
        }
        else  // If driving
        {
            targetOffset += (topSpeedOffsetChange * speedOffsetCurve.Evaluate(car.GetCarVelocityRatio()));
            targetFOV += (topSpeedFOVChange * speedOffsetCurve.Evaluate(car.GetCarVelocityRatio()));
        }

        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, targetOffset, offsetDampingSpeed * Time.deltaTime);
        cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, targetFOV, offsetDampingSpeed * Time.deltaTime);
    }
}
