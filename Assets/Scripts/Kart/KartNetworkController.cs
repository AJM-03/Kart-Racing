using UnityEngine;
using Fusion;

public class KartNetworkController : NetworkBehaviour, ISpawned
{
    private KartController kart;
    [HideInInspector] public bool spawned;
    [Networked] private double ping { get; set; }

    void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
    }

    public override void FixedUpdateNetwork()
    {
        if (kart.playerNumber == 5) return;

        if (kart.currentInput.Pause)
        {
            Debug.Log("Disconnect request recieved");
            Runner.Shutdown();
        }

        if (HasStateAuthority)
        {
            ping = Runner.GetPlayerRtt(Runner.LocalPlayer) * 1000f;  // Set the ping in milliseconds
        }
    }


    public double GetPing()
    {
        if (FusionManager.Instance == null || !Runner || !Runner.IsRunning) return 0;
        return ping;
    }
}
