using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HTestNetworkInputData : INetworkInput
{
    public Vector3 direction;

    public const byte MouseButton0 = 1;
    public const byte MouseButton1 = 2;
    public NetworkButtons buttons;
}
