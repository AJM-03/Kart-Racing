using UnityEngine;

/* Will hold an input such as constantly accelerating or driving in circles, 
 * mostly just for testing purposes
*/

public class HoldInput : KartInput
{
    [SerializeField] private bool accelInput = true;
    [SerializeField] private bool brakeInput = false;
    [SerializeField, Range(-1f, 1f)] private float steerInput;

    public override KartInputData GetInput()
    {
        data.Accelerate = accelInput;
        data.Brake = brakeInput;
        data.Steer = steerInput;

        base.GetInput();
        return data;
    }
}
