using UnityEngine;

public abstract class KartForm : MonoBehaviour
{
    [HideInInspector] public Vector3 currentCarLocalVelocity = Vector3.zero;  // The current speed of the car
    [HideInInspector] public float carVelocityRatio = 0;  // Current speed in comparison to top speed
    [HideInInspector] public bool isDrifting = false;  // Whether the car is drifting or not
    [HideInInspector] public int driftDirection;  // Which way the car is drifting (-1 = Left, 1 = Right)
    [HideInInspector] public float driftControl;  // How tightly you are drifting


    public abstract void UpdateKart();
    public abstract void FixedUpdateKart();
    public abstract void CalculateKartVelocity();
    public abstract void TransformInto(KartEnums.KartForm fromForm);
    public abstract void TransformOut(KartEnums.KartForm toForm);
}
