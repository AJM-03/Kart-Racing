using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
        if (lifetime.Expired(Runner))
        {
            Runner.Despawn(Object);
        }    
            
        transform.position += 5 * transform.forward * Runner.DeltaTime;
    }
}
