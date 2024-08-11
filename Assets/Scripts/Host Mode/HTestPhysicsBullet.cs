using Fusion;
using UnityEngine;

public class HTestPhysicsBullet : NetworkBehaviour
{
    [Networked] private TickTimer lifetime { get; set; }

    public void Init(Vector3 forward)
    {
        lifetime = TickTimer.CreateFromSeconds(Runner, 5f);
        GetComponent<Rigidbody>().velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (lifetime.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}
