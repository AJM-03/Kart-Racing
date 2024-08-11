using Fusion;
using UnityEngine;

public class HTestPlayer : NetworkBehaviour
{
    private NetworkCharacterController characterController;
    [SerializeField] private HTestBullet bulletPrefab;
    [SerializeField] private HTestPhysicsBullet physicsBulletPrefab;

    private Vector3 forward = Vector3.forward;

    [Networked] private TickTimer delay { get; set; }

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()  // Every network tick
    {
        if (GetInput(out HTestNetworkInputData data))
        {
            // Movement
            data.direction.Normalize();  // Prevents cheating with impossible inputs
            characterController.Move(10 * data.direction * Runner.DeltaTime);


            // Bullet
            if (data.direction.sqrMagnitude > 0 )
                forward = data.direction;  // Get the direction of movement

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(HTestNetworkInputData.MouseButton0))
                {
                    Runner.Spawn(bulletPrefab, transform.position + forward, Quaternion.LookRotation(forward), Object.InputAuthority,
                    (Runner, O) => { O.GetComponent<HTestBullet>().Init(); } );
                }

                else if (data.buttons.IsSet(HTestNetworkInputData.MouseButton1))
                {
                    Runner.Spawn(physicsBulletPrefab, transform.position + forward, Quaternion.LookRotation(forward), Object.InputAuthority,
                    (Runner, O) => { O.GetComponent<HTestPhysicsBullet>().Init(10 * forward); });
                }
            }
        }
    }
}
