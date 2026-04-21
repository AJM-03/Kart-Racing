using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWheelForces : MonoBehaviour
{
    public LineRenderer accelLine, steerLine, suspLine, forceLine;
    private ACarController carCont;
    
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
        suspLine.SetPosition(1, force);
    }

    public void SetSteering(Vector3 force)
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, force);
    }

    public void SetSuspension(Vector3 force)
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, force);
    }

    public void SetForce()
    {
        suspLine.SetPosition(0, transform.position);
        suspLine.SetPosition(1, accelLine.GetPosition(1) + steerLine.GetPosition(1) + suspLine.GetPosition(1));
    }
}
