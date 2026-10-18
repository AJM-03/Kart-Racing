using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    private CinemachineTransposer transposer;
    public ACarController car;

    private Vector3 stationaryCamOffset;
    public Vector3 topSpeedOffsetChange;
    private float stationaryCamFOV;
    public float topSpeedFOVChange;
    public AnimationCurve speedOffsetCurve;


    void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        transposer = cam.GetCinemachineComponent<CinemachineTransposer>();

        stationaryCamOffset = transposer.m_FollowOffset;
        stationaryCamFOV = cam.m_Lens.FieldOfView;
    }

    void Update()
    {
        Vector3 offset = stationaryCamOffset;
        offset += (topSpeedOffsetChange * speedOffsetCurve.Evaluate(car.GetCarVelocityRatio()));
        transposer.m_FollowOffset = offset;

        float fov = stationaryCamFOV;
        fov += (topSpeedFOVChange * speedOffsetCurve.Evaluate(car.GetCarVelocityRatio()));
        cam.m_Lens.FieldOfView = fov;
    }
}
