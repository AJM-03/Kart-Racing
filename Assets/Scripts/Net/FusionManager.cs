using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.UI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.SceneManagement;

public class FusionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionManager Instance;

    [HideInInspector] public NetworkRunner networkRunner;

    [SerializeField] NetworkObject playerPrefab;

    [HideInInspector] public string playerName;

    [Header("Session List")]
    private List<SessionInfo> sessions = new List<SessionInfo>();
    public GameObject nameEntryCanvas, sessionListCanvas;
    public Button refreshButton;
    public Transform sessionListContent;
    public GameObject sessionEntryPrefab;
    private bool firstLobby = false;
    private bool quickStart = false;

    [Header("Session Creation")]
    public TMP_InputField sessionNameInput;
    public TMP_InputField sessionPassword;



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Replacing Fusion Manager & Runner");
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }


    // Called after name input happens, creates network runner and connects to the network lobby
    public void ConnectToLobby(string _playerName)
    {
        playerName = _playerName;
        RefreshSessionListUI();  // Load and display the list of existing sessions
        nameEntryCanvas.SetActive(false);
        sessionListCanvas.SetActive(true);

        networkRunner ??= gameObject.AddComponent<NetworkRunner>();  // Create a network runner if there wasn't one

        networkRunner.JoinSessionLobby(SessionLobby.Shared);
    }


    // Called when the player attempts to join an existing session
    public async void ConnectToSession(string sessionName)
    {
        sessionListCanvas.SetActive(false);

        networkRunner ??= gameObject.AddComponent<NetworkRunner>();  // Create a network runner if there wasn't one

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(1),
        });
    }


    // Called when the player attempts to create a new session
    public async void CreateSession(string sessionName, string sessionPassword, int sessionPlayerCount)
    {
        //int randomInt = UnityEngine.Random.Range(1000, 9999);
        //string randomSessionName = "Room-" + randomInt.ToString();

        SessionProperty key = sessionPassword;
        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties.Add("sessionKey", key);

        networkRunner ??= gameObject.AddComponent<NetworkRunner>();  // Create a network runner if there wasn't one

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            SessionProperties = sessionProperties, 
            PlayerCount = sessionPlayerCount,
            Scene = SceneRef.FromIndex(1),
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

        if (quickStart) QuickStartConnect();
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
                script.sessionPassword = session.Properties.GetValueOrDefault("sessionKey").PropertyValue as string;
                script.privacyText.text = script.sessionPassword == "" ? "Public" : "Private";

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

    public void QuickStart()
    {
        nameEntryCanvas.SetActive(false);
        int randomNum = UnityEngine.Random.Range(1000, 9999);
        //string randomSessionName = "Room-" + randomInt.ToString();
        playerName = "Player-" + randomNum.ToString();

        networkRunner ??= gameObject.AddComponent<NetworkRunner>();  // Create a network runner if there wasn't one
        networkRunner.JoinSessionLobby(SessionLobby.Shared);
        quickStart = true;
    }

    private async void QuickStartConnect()
    {
        quickStart = false;

        // Search for an existing open session
        foreach (SessionInfo session in sessions)
        {
            if (session.IsVisible && // If visible session
                session.Properties.GetValueOrDefault("sessionKey").PropertyValue as string == "" &&  // If public
                session.PlayerCount < session.MaxPlayers && // If it is not full
                session.IsOpen)  // If it is not open
            {
                Debug.Log("Quick Joining Lobby " + session.Name);
                await networkRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = session.Name,
                    Scene = SceneRef.FromIndex(1),
                });
                return;
            }
        }

        // Else create a new one
        SessionProperty key = "";
        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties.Add("sessionKey", key);

        int randomNum = UnityEngine.Random.Range(1000, 9999);
        string randomSessionName = "Lobby-" + randomNum.ToString();
        Debug.Log("Quick Creating Lobby " + randomSessionName);

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = randomSessionName,
            SessionProperties = sessionProperties,
            PlayerCount = 8,
            Scene = SceneRef.FromIndex(1),
        });
    }

    public async void Singleplayer()
    {
        networkRunner ??= gameObject.AddComponent<NetworkRunner>();  // Create a network runner if there wasn't one
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Single,
            PlayerCount = 1,
            Scene = SceneRef.FromIndex(1),
        });
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");

        //NetworkObject playerObject = networkRunner.Spawn(playerPrefab, Vector3.zero);  // Spawn the player object
        //networkRunner.SetPlayerObject(networkRunner.LocalPlayer, playerObject);  // Tell the runner that it is the local player
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
        Debug.Log("Player joined - " + player.PlayerId);

        //if (runner.IsSceneAuthority)
        //{
        //    runner.LoadScene(SceneRef.FromIndex(1), UnityEngine.SceneManagement.LoadSceneMode.Single);
        //}
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player left - " + player.PlayerId);
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
        Debug.Log("Server shutdown " + shutdownReason.ToString());

    }

    public void OnUserSimulationMessage(NetworkRunner runner)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
    {

    }
}
