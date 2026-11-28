using Cinemachine;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class KartInitializer : MonoBehaviour
{
    private KartController kart;
    [HideInInspector] public bool initialized = false;

    private void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
    }

    private void Start()
    {
        if (!kart.IsLocalPlayer()) Initialize(kart.playerNumber);  // If the kart is a bot or net racer
    }


    public void Initialize(int playerNumber)
    {
        if (initialized) return;
        SetPlayerNumber(playerNumber);
        SetInput();
        SetCam();
        kart.canMove = true;
        kart.ChangeForm(KartEnums.KartForm.Car, true);
        kart.rb = GetComponentInParent<Rigidbody>();
        SetCenterOfMass();
        SetKartName();
        initialized = true;
        Debug.Log("Initialized Kart " + kart.playerNumber);
    }


    private void SetPlayerNumber(int playerNumber)
    {
        Debug.Log(playerNumber + "  -  " + kart.GetComponentInChildren<KartNetworkController>().HasStateAuthority);
        if (playerNumber == 0 && !kart.GetComponentInChildren<KartNetworkController>().HasStateAuthority) playerNumber = 6;  // Detecting network racers
        kart.playerNumber = playerNumber;
    }


    private void SetInput()
    {
        kart.TryGetComponent<PlayerInputHandler>(out PlayerInputHandler playerInput);

        if (kart.IsLocalPlayer())
        {
            if (playerInput) playerInput.enabled = true;
            if (playerInput) playerInput.InitializePlayer(LocalPlayerConfigurationManager.Instance.GetPlayerConfigs()[kart.playerNumber - 1]);
        }
        else
        {
            if (playerInput) playerInput.enabled = false;
        }
    }


    private void SetCam()
    {
        Camera cam = transform.GetComponentInChildren<Camera>();
        if (cam == null) return;

        int player = kart.playerNumber;
        int players = LocalPlayerConfigurationManager.Instance.GetPlayerCount();

        if (players == 1)
        {
            if (player != 1) 
                Destroy(cam.transform.parent.gameObject);
        }

        if (players == 2)
        {
            if (player == 1 || player == 2)
            {
                cam.rect = new Rect(0.5f * (player - 1), 0, 0.5f, 1);
            }
            else
                Destroy(cam.transform.parent.gameObject);
        }

        if (players == 3 || players == 4)
        {
            if (player >= 1 && player <= 4)
            {
                cam.rect = new Rect(0.5f * ((player - 1) % 2), 0, 0.5f, 0.5f);
            }
            else
                Destroy(cam.transform.parent.gameObject);
        }
    }


    private void SetKartName()
    {
        if (kart.IsLocalPlayer()) kart.gameObject.name = "Kart " + kart.racerIndex + " (Player " + kart.playerNumber + ")";
        else if (kart.playerNumber == 5) kart.gameObject.name = "Kart " + kart.racerIndex + " (Bot)";
        else if (kart.playerNumber == 6) kart.gameObject.name = "Kart " + kart.racerIndex + " (Net)";
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
