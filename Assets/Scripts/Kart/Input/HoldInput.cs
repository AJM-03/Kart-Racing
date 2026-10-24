using UnityEngine;

public class HoldInput : KartInput
{
    [HideInInspector, Range(-1f, 1f)] private float accelInput = 1f;
    [HideInInspector, Range(-1f, 1f)] private float steerInput;

    public override KartInputData GetInput()
    {
        data.Accelerate = accelInput;
        data.Steer = steerInput;

        return data;
    }
}
