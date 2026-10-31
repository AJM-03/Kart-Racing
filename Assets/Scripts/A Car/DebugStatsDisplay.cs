using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugStatsDisplay : MonoBehaviour
{
    public List<TMP_Text> text = new List<TMP_Text>();

    void Update()
    {
        foreach (var t in text) t.text = "";

        for (int i = 0; i < DebugStats.carStats.Count; i++)
        {
            if (text.Count > i)
            {
                text[i].text = @$"
Player: {i + 1} ~ {DebugStats.carStats[i].playerNumber}          
MoveInput: {DebugStats.carStats[i].moveInput}
SteerInput: {DebugStats.carStats[i].steerInput}
Braking: {DebugStats.carStats[i].braking}
Velocity: {DebugStats.carStats[i].currentCarLocalVelocity} ~ {DebugStats.carStats[i].currentCarLocalVelocity.magnitude.Round(2)}
VelocityRatio: {DebugStats.carStats[i].carVelocityRatio:F2}
GroundedWheels: {DebugStats.carStats[i].groundedWheels}
                ";
            }
        }
    }
}