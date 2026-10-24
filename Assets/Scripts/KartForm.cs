using UnityEngine;

public abstract class KartForm : MonoBehaviour
{
    public abstract void UpdateKart();
    public abstract void FixedUpdateKart();
    public abstract void TransformInto(KartEnums.KartForm fromForm);
    public abstract void TransformOut(KartEnums.KartForm toForm);
}
