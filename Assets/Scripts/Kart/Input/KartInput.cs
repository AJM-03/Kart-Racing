using Fusion;
using UnityEngine;
using static UnityEditor.Progress;

/* An abstract form of the kart input, will be overriden to allow different forms 
 * of input such as from the local player, network player or AI
*/

public struct KartInputData : INetworkStruct
{
    public bool Accelerate;  // The player's current accel input 
    public bool Brake;  // The player's current brake input 
    public float Steer;  // The player's current steering input
    public bool Drift;  // The player's current drift input 
    public bool Item;  // The player's current item input 
    public bool Transform;  // The player's current transform input 
    public bool Pause;  // The player's current drift input 

    public KartInputData(bool accelerate, bool brake, float steer, bool drift, bool item, bool transform, bool pause)
    {
        Accelerate = accelerate;
        Brake = brake;
        Steer = steer;
        Drift = drift;
        Item = item;
        Transform = transform;
        Pause = pause;
    }
}

public abstract class KartInput : NetworkBehaviour
{
    [Networked] public KartInputData data { get; set; }
    private KartController kart;


    private void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
    }

    public virtual KartInputData GetInput()
    {
        if (!kart.canMove) ClearInput(true);  // If the car should not be able to move, clear the input

        return data;
    }

    public virtual void ClearInput(bool allowPause)
    {
        data = new KartInputData(
        false,  // Accel
        false,  // Brake
        0,      // Steer
        false,  // Drift
        false,  // Item
        false,  // Transform
        allowPause ? data.Pause : false  // Gives the option of leaving the pause button active
        );
    }
}
