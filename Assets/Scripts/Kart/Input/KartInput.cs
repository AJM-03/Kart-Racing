using UnityEngine;
using System;
using System.Collections;

public struct KartInputData
{
    public float Accelerate;  // The player's current accel & decel input 
    public float Steer;  // The player's current steering input

    public KartInputData(float accelerate = 0, float steer = 0)
    {
        Accelerate = accelerate;
        Steer = steer;
    }
}

public abstract class KartInput : MonoBehaviour
{
    public KartInputData data;

    public abstract KartInputData GetInput();
}
