using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.UI;
using JetBrains.Annotations;
using TMPro;

public class FusionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionManager Instance;

    [HideInInspector] public NetworkRunner networkRunner;

    [SerializeField] NetworkObject playerPrefab;

    [HideInInspector] public string playerName;

    [Header("Session List")]
    private List<SessionInfo> sessions = new List<SessionInfo>();
    public GameObject sessionListCanvas;
    public Button refreshButton;
    public Transform sessionListContent;
    public GameObject sessionEntryPrefab;
    private bool firstLobby = false;

    [Header("Session Creation")]
    public TMP_InputField sessionNameInput;
    public TMP_InputField sessionPassword;



    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }


    public void ConnectToLobby(string _playerName)
    {
        playerName = _playerName;
        RefreshSessionListUI();
        sessionListCanvas.SetActive(true);

        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>(); 
        }

        networkRunner.JoinSessionLobby(SessionLobby.Shared);
    }


    public async void ConnectToSession(string sessionName)
    {
        sessionListCanvas.SetActive(false);

        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
        }

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
        });
    }


    public async void CreateSession(string sessionName, string sessionPassword, int sessionPlayerCount)
    {
        //int randomInt = UnityEngine.Random.Range(1000, 9999);
        //string randomSessionName = "Room-" + randomInt.ToString();

        SessionProperty key = sessionPassword;
        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties.Add("sessionKey", key);

        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
        }

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            SessionProperties = sessionProperties, 
            PlayerCount = sessionPlayerCount,
        });
    }


    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        sessions.Clear();
        sessions = sessionList;

        if (!firstLobby)
        {
            RefreshSessionListUI();
            firstLobby = true;
        }
    }


    public void RefreshSessionListUI()
    {
        foreach (Transform child in sessionListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (SessionInfo session in sessions)
        {
            if (session.IsVisible)
            {
                GameObject entry = GameObject.Instantiate(sessionEntryPrefab, sessionListContent);
                entry.transform.parent = sessionListContent;
                SessionEntry script = entry.GetComponent<SessionEntry>();
                script.sessionName.text = session.Name;
                script.playerCount.text = session.PlayerCount + "/" + session.MaxPlayers;
                script.privacyText.text = script.sessionPassword == "" ? "Private" : "Public";
                script.sessionPassword = session.Properties.GetValueOrDefault("sessionKey").PropertyValue as string;

                if (!session.IsOpen || session.PlayerCount >= session.MaxPlayers)
                {
                    script.joinButton.interactable = false;
                }
                else
                {
                    script.joinButton.interactable = true;
                }
            }
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");

        NetworkObject playerObject = networkRunner.Spawn(playerPrefab, Vector3.zero);  // Spawn the player object
        networkRunner.SetPlayerObject(networkRunner.LocalPlayer, playerObject);  // Tell the runner that it is the local player
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError("Server connection failed");

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("Connect request");

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected to server");

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("Host migrating");

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player joined");

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player left");

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Server shutdown");

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
}
