using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    private KartController kart;
    private Car car;
    private Wheels wheels;

    void Start()
    {
        kart = GetComponentInParent<KartController>();
        if (kart) car = kart.car;
        if (car) wheels = car.wheels;
        if (wheels == null) Debug.LogError("Kart references not found by " + this);

        for (int i = 0; i < tires.Length; i++) tirePositions[i] = tires[i].transform.parent.localPosition;
    }

    void Update()
    {
        
    }
}
