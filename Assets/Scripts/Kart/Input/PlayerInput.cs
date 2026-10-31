using UnityEngine;

// Will send the local player's input to the kart

public class PlayerInput : KartInput
{
    public override KartInputData GetInput()
    {
        //data.Accelerate = Input.GetButton("Fire2") ? 1 : (Input.GetButton("Fire1") ? -1 : 0);
        data.Accelerate = Input.GetButton("Fire2");
        data.Brake = Input.GetButton("Fire1");
        data.Steer = Input.GetAxis("Horizontal");
        data.Drift = Input.GetButton("Jump");


        base.GetInput();
        return data;
    }
}
