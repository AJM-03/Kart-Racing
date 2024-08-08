using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STestLookAtCamera : MonoBehaviour
{
    private GameObject mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.gameObject;
    }

    private void Update()
    {
        transform.LookAt(mainCamera.transform);
    }
}
