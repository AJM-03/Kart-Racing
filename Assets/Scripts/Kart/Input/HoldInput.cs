using UnityEngine;
using static UnityEngine.InputSystem.DefaultInputActions;

/* Will hold an input such as constantly accelerating or driving in circles, 
 * mostly just for testing purposes
*/

public class HoldInput : KartInput
{
    [SerializeField] private bool accelInput = true;
    [SerializeField] private bool brakeInput = false;
    [SerializeField, Range(-1f, 1f)] private float steerInput;
    [SerializeField] private bool driftInput = false;


    public override KartInputData GetInput()
    {
        data = new KartInputData(
            accelInput,
            brakeInput,
            steerInput,
            driftInput,
            false,  // Item
            false,  // Transform
            false   // Pause
        );

        base.GetInput();
        return data;
    }
}
