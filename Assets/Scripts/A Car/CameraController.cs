using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    private KartController kart;
    private Transform lookAt;
    private CinemachineVirtualCamera cam;
    private CinemachineTransposer transposer;
    private CinemachineOrbitalTransposer orbitalTransposer;


    [Header("Damping")]
    public float offsetDampingSpeed;

    [Header("Starting & Target Values")]
    private Vector3 stationaryCamOffset;
    private Vector3 targetOffset;
    private float stationaryCamFOV;
    private float targetFOV;
    private Vector3 lookAtStartingPos;
    private Vector3 lookAtTargetPos;


    [Header("Speed")]
    public AnimationCurve speedOffsetCurve;
    public Vector3 topSpeedOffsetChange;
    public float topSpeedFOVChange;

    [Header("Braking")]
    public AnimationCurve brakingOffsetCurve;
    public Vector3 brakingOffsetChange;
    public float brakingFOVChange;

    [Header("Turning")]
    public Vector3 driftingOffsetChange;
    public Vector3 lookAtOffsetChange;
    //[SerializeField, Range(0f, 5f)] private float cameraHeadingChange = 0.75f;  // How far the camera will move left and right when steering
    //[SerializeField, Range(0f, 5f)] private float driftCameraHeadingChange = 0.75f;  // How far the camera will move left and right when drifting


    void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);

        cam = GetComponent<CinemachineVirtualCamera>();
        transposer = cam.GetCinemachineComponent<CinemachineTransposer>();
        orbitalTransposer = cam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        lookAt = transposer.LookAtTarget;

        stationaryCamOffset = transposer.m_FollowOffset;
        stationaryCamFOV = cam.m_Lens.FieldOfView;
        lookAtStartingPos = lookAt.localPosition;
    }

    void Update()
    {
        targetOffset = stationaryCamOffset;
        targetFOV = stationaryCamFOV;
        lookAtTargetPos = lookAtStartingPos;

        if (kart.IsBraking())  // If braking
        {
            targetOffset += (brakingOffsetChange * brakingOffsetCurve.Evaluate(kart.GetVelocityRatio()));
            targetFOV += (brakingFOVChange * brakingOffsetCurve.Evaluate(kart.GetVelocityRatio()));
        }

        else  // If driving
        {
            targetOffset += (topSpeedOffsetChange * speedOffsetCurve.Evaluate(kart.GetVelocityRatio()));
            targetFOV += (topSpeedFOVChange * speedOffsetCurve.Evaluate(kart.GetVelocityRatio()));

            if (kart.IsDrifting())  // If drifting
            {
                targetOffset += (driftingOffsetChange * speedOffsetCurve.Evaluate(kart.GetVelocityRatio()) * kart.GetDriftDirection());
                lookAtTargetPos += (lookAtOffsetChange * speedOffsetCurve.Evaluate(kart.GetVelocityRatio()) * kart.GetDriftDirection());
            }
        }

        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, targetOffset, offsetDampingSpeed * Time.deltaTime);
        cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, targetFOV, offsetDampingSpeed * Time.deltaTime);
        lookAt.localPosition = Vector3.Lerp(lookAt.localPosition, lookAtTargetPos, offsetDampingSpeed * Time.deltaTime);





        //float turnAmount = car.GetSteerInput();
        //if (car.IsDrifting()) turnAmount = car.GetDriftControl() * car.GetDriftDirection();
        //if (!car.IsGrounded()) turnAmount = 0;
        //orbitalTransposer.m_Heading.m_Bias = turnAmount * car.GetCarVelocityRatio() * (car.IsDrifting() ? driftCameraHeadingChange : cameraHeadingChange);
    }
}
