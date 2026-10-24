using UnityEngine;

public abstract class KartForm : MonoBehaviour
{
    [HideInInspector] public Vector3 currentCarLocalVelocity = Vector3.zero;  // The current speed of the car
    [HideInInspector] public float carVelocityRatio = 0;  // Current speed in comparison to top speed
    public abstract void UpdateKart();
    public abstract void FixedUpdateKart();
    public abstract void CalculateKartVelocity();
    public abstract void TransformInto(KartEnums.KartForm fromForm);
    public abstract void TransformOut(KartEnums.KartForm toForm);
}
