using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugStatsDisplay : MonoBehaviour
{
    public List<TMP_Text> text = new List<TMP_Text>();

    public void OnDestroy()
    {
        DebugStats.carStats.Clear();
    }

    void Update()
    {
        foreach (var t in text) t.text = "";

        int textIndex = 0;

        // Print all real players
        for (int i = 0; i < DebugStats.carStats.Count; i++)
        {
            if (DebugStats.carStats[i] == null || DebugStats.carStats[i].targetKart == null) continue;
            if (DebugStats.carStats[i].targetKart.GetComponent<KartController>().playerNumber == 5) continue;
            if (textIndex >= text.Count) return;

            PrintDebugText(i, textIndex);
            textIndex++;
        }

        // Print all AI players
        for (int i = 0; i < DebugStats.carStats.Count; i++)
        {
            if (DebugStats.carStats[i] == null || DebugStats.carStats[i].targetKart == null) continue;
            if (DebugStats.carStats[i].targetKart.GetComponent<KartController>().playerNumber != 5) continue;
            if (textIndex >= text.Count) return;

            PrintDebugText(i, textIndex);
            textIndex++;
        }
    }

    private void PrintDebugText(int carIndex, int textIndex)
    {
        text[textIndex].text = @$"
Player: {carIndex + 1} ~ {DebugStats.carStats[carIndex].playerNumber}          
MoveInput: {DebugStats.carStats[carIndex].moveInput}
SteerInput: {DebugStats.carStats[carIndex].steerInput}
Braking: {DebugStats.carStats[carIndex].braking}
Velocity: {DebugStats.carStats[carIndex].currentCarLocalVelocity} ~ {DebugStats.carStats[carIndex].currentCarLocalVelocity.magnitude.Round(2)}
VelocityRatio: {DebugStats.carStats[carIndex].carVelocityRatio:F2}
GroundedWheels: {DebugStats.carStats[carIndex].groundedWheels}
Ping: {DebugStats.carStats[carIndex].targetKart.GetComponentInChildren<KartNetworkController>().GetPing():F2} ms
                ";
    }
}

