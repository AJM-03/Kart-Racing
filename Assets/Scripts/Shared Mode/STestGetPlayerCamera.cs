using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using StarterAssets;

public class STestGetPlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraLookAt;

    private void Start()
    {
        NetworkObject thisObject = GetComponent<NetworkObject>();

        if (thisObject.HasInputAuthority)
        {
            GameObject virtualCamera = GameObject.Find("PlayerFollowCamera");
            virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = cameraLookAt;
        }

        if (thisObject.HasInputAuthority || thisObject.HasStateAuthority)
        {
            GetComponent<ThirdPersonController>().enabled = true;
        }
    }
}
