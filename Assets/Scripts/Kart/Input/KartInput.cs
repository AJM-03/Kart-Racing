using UnityEngine;

/* An abstract form of the kart input, will be overriden to allow different forms 
 * of input such as from the local player, network player or AI
*/

public struct KartInputData
{
    public bool Accelerate;  // The player's current accel input 
    public bool Brake;  // The player's current brake input 
    public float Steer;  // The player's current steering input

    public KartInputData(bool accelerate, bool brake, float steer = 0)
    {
        Accelerate = accelerate;
        Brake = brake;
        Steer = steer;
    }
}

public abstract class KartInput : MonoBehaviour
{
    public KartInputData data;
    private KartController kart;


    private void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
    }

    public virtual KartInputData GetInput()
    {
        if (!kart.canMove) data = new KartInputData();  // If the car should not be able to move, clear the input

        return data;
    }
}
