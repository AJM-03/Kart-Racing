using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugStats
{
    public static List<CarDebugStats> carStats = new List<CarDebugStats>();

}

public class CarDebugStats
{
    public float moveInput;
    public float steerInput;
    public bool braking;
    public Vector3 currentCarLocalVelocity;
    public float carVelocityRatio;
    public int groundedWheels;
}