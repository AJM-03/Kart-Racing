using Fusion;
using UnityEngine;

public class HTestBullet : NetworkBehaviour
{
    [Networked] private TickTimer lifetime { get; set; }

    public void Init()
    {
        lifetime = TickTimer.CreateFromSeconds(Runner, 5f);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += 5 * transform.forward * Runner.DeltaTime;
        
        if (lifetime.Expired(Runner))
        {
            Runner.Despawn(Object);
        }    
    }
}
