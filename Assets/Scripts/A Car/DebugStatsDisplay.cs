using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugStatsDisplay : MonoBehaviour
{
    public TMP_Text text;

    void Update()
    {
        text.text = @$"
MoveInput: {DebugStats.moveInput}
SteerInput: {DebugStats.steerInput}
CurrentCarVelocity: {DebugStats.currentCarLocalVelocity}
CarVelocityRatio: {DebugStats.carVelocityRatio}
GroundedWheels: {DebugStats.groundedWheels}
";
    }
}