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
        if (kart.racerIndex == 0)
        {
            DebugStats.carStats.Add(new CarDebugStats());
            kart.racerIndex = DebugStats.carStats.Count;
        }

        DebugStats.carStats[kart.racerIndex - 1].moveInput = kart.currentInput.Accelerate;
        if (kart.IsDrifting()) DebugStats.carStats[kart.racerIndex - 1].steerInput = kart.GetDriftControl() * kart.GetDriftDirection();
        else DebugStats.carStats[kart.racerIndex - 1].steerInput = kart.currentInput.Steer;
        DebugStats.carStats[kart.racerIndex - 1].braking = kart.IsBraking();
        DebugStats.carStats[kart.racerIndex - 1].currentCarLocalVelocity = kart.GetVelocity();
        DebugStats.carStats[kart.racerIndex - 1].carVelocityRatio = kart.GetVelocityRatio();

        int gW = 0;
        for (int i = 0; i < kart.car.wheels.groundedWheels.Length; i++) { gW += kart.car.wheels.groundedWheels[i]; }
        DebugStats.carStats[kart.racerIndex - 1].groundedWheels = gW;
    }
}
