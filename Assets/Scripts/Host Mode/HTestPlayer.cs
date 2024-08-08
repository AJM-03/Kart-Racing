using Fusion;
using UnityEngine;

public class HTestPlayer : NetworkBehaviour
{
    private NetworkCharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()  // Every network tick
    {
        if (GetInput(out HTestNetworkInputData data))
        {
            data.direction.Normalize();
            characterController.Move(10 * data.direction * Runner.DeltaTime);
        }
    }
}
