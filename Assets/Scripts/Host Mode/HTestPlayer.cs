using Fusion;
using UnityEngine;
using UnityEngine.Windows;

public class HTestPlayer : NetworkBehaviour
{
    private NetworkCharacterController characterController;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject physicsBulletPrefab;
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }

    private Vector3 forward = Vector3.forward;

    [Networked] private TickTimer delay { get; set; }

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()  // Every network tick
    {
        if (GetInput(out HTestNetworkInputData data) == false) return;

        // compute pressed/released state
        NetworkButtons pressed = data.buttons.GetPressed(ButtonsPrevious);
        NetworkButtons released = data.buttons.GetReleased(ButtonsPrevious);

        // store latest input as 'previous' state we had
        ButtonsPrevious = data.buttons;


        // Movement
        data.direction.Normalize();  // Prevents cheating with impossible inputs
        characterController.Move(10 * data.direction * Runner.DeltaTime);


        // Bullet
        if (data.direction.sqrMagnitude > 0 )
            forward = data.direction;  // Get the direction of movement

        if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
        {
            if (pressed.IsSet(MyButtons.Ball))
            {
                Runner.Spawn(bulletPrefab, transform.position + forward, Quaternion.LookRotation(forward), Object.InputAuthority,
                (Runner, O) => { O.GetComponent<HTestBullet>().Init(); } );
            }

            if (pressed.IsSet(MyButtons.PhysBall))
            {
                Runner.Spawn(physicsBulletPrefab, transform.position + forward, Quaternion.LookRotation(forward), Object.InputAuthority,
                (Runner, O) => { O.GetComponent<HTestPhysicsBullet>().Init(10 * forward); });
            }
        }
    }
}
