using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MyButtons
{
    Ball = 0,
    PhysBall = 1,
}


public struct HTestNetworkInputData : INetworkInput
{
    public Vector3 direction;
    public NetworkButtons buttons;
}
