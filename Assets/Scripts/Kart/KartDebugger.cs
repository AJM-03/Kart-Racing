using UnityEngine;

public class KartDebugger : MonoBehaviour
{
    private KartController kart;

    void Awake()
    {
        kart = GetComponentInParent<KartController>();
        if (kart == null) Debug.LogError("Kart Controller not found by " + this);
        UpdateDebugStats();
    }

    void Update()
    {
        UpdateDebugStats();
    }

    private void UpdateDebugStats()
    {
        if (kart.playerNumber == 0)
        {
            DebugStats.carStats.Add(new CarDebugStats());
            kart.playerNumber = DebugStats.carStats.Count;
        }

        DebugStats.carStats[kart.playerNumber - 1].moveInput = kart.currentInput.Accelerate;
        if (isDrifting) DebugStats.carStats[kart.playerNumber - 1].steerInput = driftControl * driftDirection;
        else DebugStats.carStats[kart.playerNumber - 1].steerInput = kart.currentInput.Steer;
        DebugStats.carStats[kart.playerNumber - 1].braking = isBraking;
        DebugStats.carStats[kart.playerNumber - 1].currentCarLocalVelocity = kart.GetKartVelocity();
        DebugStats.carStats[kart.playerNumber - 1].carVelocityRatio = kart.GetKartVelocityRatio();

        int gW = 0;
        for (int i = 0; i < groundedWheels.Length; i++) { gW += groundedWheels[i]; }
        DebugStats.carStats[kart.playerNumber - 1].groundedWheels = gW;
    }
}
