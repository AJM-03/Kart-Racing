using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class KartInitializer : MonoBehaviour
{
    private KartController kart;

    void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);

        Camera cam = transform.GetComponentInChildren<Camera>();
        if (cam && kart.playerNumber > 4) Destroy(cam.transform.parent.gameObject);

        kart.ChangeForm(KartEnums.KartForm.Car, true);
        SetCenterOfMass();
    }



    private void SetCenterOfMass()  // Initializes the kart's rigidbody by setting the centre of mass
    {
        float x = 0;
        float z = 0;
        foreach (Transform point in kart.car.wheels.rayPoints)
        {
            x += point.localPosition.x;
            z += point.localPosition.z;
        }
        kart.rb.centerOfMass = new Vector3(x, -0.25f, z); // Lower the center of mass.
    }
}
