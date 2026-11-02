using UnityEngine;
using Fusion;

public class KartNetworkController : NetworkBehaviour
{
    private KartController kart;

    void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
    }

    private void Update()
    {
        if (kart.currentInput.Disconnect)
        {
            Debug.Log("Disconnect request recieved");
            Runner.Shutdown();
        }
    }
}
