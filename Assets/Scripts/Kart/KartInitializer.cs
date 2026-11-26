using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class KartInitializer : MonoBehaviour
{
    private KartController kart;

    private void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);


        if (kart.playerNumber > 4) Initialize(kart.playerNumber);  // If the kart is a bot
    }


    public void Initialize(int playerNumber)
    {
        kart.playerNumber = playerNumber;

        Camera cam = transform.GetComponentInChildren<Camera>();
        if (cam && kart.playerNumber > 4) Destroy(cam.transform.parent.gameObject);

        SetInput();
        kart.canMove = true;
        kart.ChangeForm(KartEnums.KartForm.Car, true);
        kart.rb ??= GetComponentInParent<Rigidbody>();
        SetCenterOfMass();

        kart.gameObject.name = "Kart (Player " + kart.playerNumber + ")";
    }



    private void SetInput()
    {
        if (kart.playerNumber >= 1 && kart.playerNumber <= 4)
        {
            kart.GetComponentInChildren<PlayerInputHandler>().enabled = true;
            kart.GetComponentInChildren<PlayerInputHandler>().InitializePlayer(LocalPlayerConfigurationManager.Instance.GetPlayerConfigs()[kart.playerNumber - 1]);
        }
        else
        {
            kart.GetComponentInChildren<PlayerInputHandler>().enabled = false;
        }
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
