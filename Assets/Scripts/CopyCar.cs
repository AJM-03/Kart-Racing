using Cinemachine;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCar : MonoBehaviour
{

    #region References
    public bool IsGrounded() { return isGrounded; }
    public bool IsBraking() { return isBraking; }
    public bool IsDrifting() { return isDrifting; }
    public bool IsBrakeDrifting() { return isDrifting && Input.GetButton("Fire1"); }
    public float GetSteerInput() { return steerInput; }
    public float GetDriftDirection() { return driftDirection; }
    public float GetDriftControl() { return driftControl; }

    #endregion
}


