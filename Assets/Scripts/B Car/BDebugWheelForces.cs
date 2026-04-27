using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BDebugWheelForces : MonoBehaviour
{
    public LineRenderer accelLine, steerLine, suspLine, forceLine;
    private BCarController carCont;
    
    void Start()
    {
        accelLine.gameObject.SetActive(DebugStats.showWheelForces);
        steerLine.gameObject.SetActive(DebugStats.showWheelForces);
        suspLine.gameObject.SetActive(DebugStats.showWheelForces);
        forceLine.gameObject.SetActive(DebugStats.showWheelForces);
    }

    public void SetAcceleration(Vector3 force)
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, force / 1000);
    }

    public void SetSteering(Vector3 force)
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, force / 1000);
    }

    public void SetSuspension(Vector3 force)
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, force / 1000);
    }

    public void SetForce()
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, accelLine.GetPosition(1) + steerLine.GetPosition(1) + suspLine.GetPosition(1));
    }
}
