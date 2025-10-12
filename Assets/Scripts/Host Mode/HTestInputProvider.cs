using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEngine.InputSystem.DefaultInputActions;

public class HTestInputProvider : SimulationBehaviour, INetworkRunnerCallbacks
{
    // creating a instance of the Input Action created
    private TestControls _playerActionMap;
    HTestNetworkInputData inputData = new HTestNetworkInputData();


    public void Awake()
    {
        EnableActionMap();
    }

    public void OnEnable()
    {
        if (Runner != null)
        {
            EnableActionMap();
            Runner.AddCallbacks(this);
        }
    }

    private void EnableActionMap()
    {
        if (_playerActionMap == null) _playerActionMap = new TestControls();
        _playerActionMap.Movement.Enable();
    }

    public void Update()
    {
        var playerActions = _playerActionMap.Movement;

        inputData.direction.Set(playerActions.Move.ReadValue<Vector2>().x, 0, playerActions.Move.ReadValue<Vector2>().y);
        inputData.look.Set(playerActions.Look.ReadValue<Vector2>().x, 0, playerActions.Look.ReadValue<Vector2>().y);

        if (playerActions.Jump.IsPressed())
            inputData.buttons.Set(MyButtons.Jump, true);
        if (playerActions.Sprint.IsPressed())
            inputData.buttons.Set(MyButtons.Sprint, true);
        if (playerActions.Ball.IsPressed())
            inputData.buttons.Set(MyButtons.Ball, true);
        if (playerActions.PhysicsBall.IsPressed())
            inputData.buttons.Set(MyButtons.PhysBall, true);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(inputData);
        inputData = default;
    }

    public void OnDisable()
    {
        if (Runner != null)
        {
            // disabling the input map
            _playerActionMap.Movement.Disable();
            inputData = default;
            Runner.RemoveCallbacks(this);
        }
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
}
