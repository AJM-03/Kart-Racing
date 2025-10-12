using Fusion;
using StarterAssets;
using UnityEngine;
using UnityEngine.Windows;

public class HTestPlayer : NetworkBehaviour
{
    private ThirdPersonController characterController;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject physicsBulletPrefab;
    public Transform shootPos;
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }

    private Vector3 forward = Vector3.forward;

    [Networked] private TickTimer delay { get; set; }

    private void Awake()
    {
        characterController = GetComponent<ThirdPersonController>();
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
        //data.direction.Normalize();  // Prevents cheating with impossible inputs
        //characterController.Move(10 * data.direction * Runner.DeltaTime);

        characterController.moveInput.x = data.direction.x;
        characterController.moveInput.y = data.direction.z;

        characterController.lookInput.x = data.look.x;
        characterController.lookInput.y = data.look.z;


        // Bullet
        //if (data.direction.sqrMagnitude > 0 )
        //    forward = data.direction;  // Get the direction of movement
        forward = transform.forward;

        if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
        {
            if (data.buttons.IsSet(MyButtons.Jump))
            {
                characterController.jumpInput = true;
            }

            if (data.buttons.IsSet(MyButtons.Sprint))
            {
                characterController.sprintInput = true;
            }

            if (pressed.IsSet(MyButtons.Ball))
            {
                Runner.Spawn(bulletPrefab, shootPos.position, Quaternion.LookRotation(forward), Object.InputAuthority,
                (Runner, O) => { O.GetComponent<HTestBullet>().Init(); } );
            }

            if (pressed.IsSet(MyButtons.PhysBall))
            {
                Runner.Spawn(physicsBulletPrefab, shootPos.position, Quaternion.LookRotation(forward), Object.InputAuthority,
                (Runner, O) => { O.GetComponent<HTestPhysicsBullet>().Init(10 * forward); });
            }
        }
    }
}
