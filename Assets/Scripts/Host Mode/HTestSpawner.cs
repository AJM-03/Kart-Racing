using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HTestSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner networkRunner;
    [SerializeField] private NetworkPrefabRef playerPrefabRef;

    private Dictionary<PlayerRef, NetworkObject> spawnCharacter = new Dictionary<PlayerRef, NetworkObject> ();

    public static HTestSpawner Instance;

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
        gameObject.AddComponent(typeof(HTestInputProvider));
    }

    public void ConnectToLobby(string _playerName)  // Open the session list
    {
        playerName = _playerName;
        RefreshSessionListUI();
        sessionListCanvas.SetActive(true);

        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
        }

        networkRunner.JoinSessionLobby(SessionLobby.ClientServer);
    }


    public async void ConnectToSession(string sessionName)  // Select a session
    {
        sessionListCanvas.SetActive(false);

        // Create runner and give user input
        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
            gameObject.AddComponent<RunnerSimulatePhysics3D>();
            networkRunner.ProvideInput = true;
        }


        // Get scene info
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);


        // Create Session
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });
    }


    public async void CreateSession(string sessionName, string sessionPassword, int sessionPlayerCount)
    {
        // Create runner and give user input
        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
            gameObject.AddComponent<RunnerSimulatePhysics3D>().ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateForward; ;
            networkRunner.ProvideInput = true;
        }


        // Set session password
        SessionProperty key = sessionPassword;
        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties.Add("sessionKey", key);


        // Get scene info
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);


        // Create Session
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = sessionProperties,
            PlayerCount = sessionPlayerCount
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
                HTestSessionEntry script = entry.GetComponent<HTestSessionEntry>();
                script.sessionName.text = session.Name;
                script.playerCount.text = session.PlayerCount + "/" + session.MaxPlayers;
                Debug.Log(session.Properties.GetValueOrDefault("sessionKey"));
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

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
    }


    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
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
        Debug.Log("Player Joined");

        if (runner.IsServer)
        {
            Vector3 playerPos = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1f, 0f);

            NetworkObject networkObject = runner.Spawn(playerPrefabRef, playerPos, Quaternion.identity, player);

            spawnCharacter.Add(player, networkObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left");

        if (spawnCharacter.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            spawnCharacter.Remove(player);
        }
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
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
}

/*
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HTestSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner networkRunner;
    [SerializeField] private NetworkPrefabRef playerPrefabRef;

    private Dictionary<PlayerRef, NetworkObject> spawnCharacter = new Dictionary<PlayerRef, NetworkObject>();

    async void GameStart(GameMode mode)
    {
        // Create runner and give user input
        networkRunner = gameObject.AddComponent<NetworkRunner>();
        gameObject.AddComponent<RunnerSimulatePhysics3D>().ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateForward; ;
        networkRunner.ProvideInput = true;

        // Get scene info
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        // Create Session
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void OnGUI()
    {
        if (networkRunner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                GameStart(GameMode.Host);
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
                GameStart(GameMode.Client);
        }
    }

    private bool _mouseButton0;
    private bool _mouseButton1;

    private void Update()
    {
        _mouseButton0 = _mouseButton0 | Input.GetMouseButtonDown(0);
        _mouseButton1 = _mouseButton1 | Input.GetMouseButtonDown(1);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new HTestNetworkInputData();

        if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;

        // Add Structure & Host
        data.buttons.Set(HTestNetworkInputData.MouseButton0, _mouseButton0);
        _mouseButton0 = false;

        data.buttons.Set(HTestNetworkInputData.MouseButton1, _mouseButton1);
        _mouseButton1 = false;

        input.Set(data);  // Pass data to the host
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
        Debug.Log("Player Joined");

        if (runner.IsServer)
        {
            Vector3 playerPos = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1f, 0f);

            NetworkObject networkObject = runner.Spawn(playerPrefabRef, playerPos, Quaternion.identity, player);

            spawnCharacter.Add(player, networkObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left");

        if (spawnCharacter.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            spawnCharacter.Remove(player);
        }
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

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
}
*/