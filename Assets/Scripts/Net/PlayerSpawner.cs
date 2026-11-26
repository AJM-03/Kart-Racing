using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour
{
    public GameObject playerPrefab;
    public Vector3 spawnPoint;

    [SerializeField] private GameObject[] spawnedKarts;
    private int spawnedKartIndex;

    private void Awake()
    {
        var runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (FusionManager.Instance == null || !runner.IsRunning) return;
            
        runner.AddGlobal(this);
        spawnedKarts = new GameObject[4];
        spawnedKartIndex = 0;

        //NetworkSpawnOp playerObject = Runner.SpawnAsync(playerPrefab, spawnPoint, Quaternion.identity);  // Spawn the player object
        //Runner.SetPlayerObject(Runner.LocalPlayer, playerObject.Object);  // Tell the runner that it is the local player

        var playerConfigs = LocalPlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            NetworkSpawnOp playerObject = Runner.SpawnAsync(playerPrefab, spawnPoint, Quaternion.identity, null, OnBeforeSpawned);  // Spawn the player object
            Runner.SetPlayerObject(Runner.LocalPlayer, playerObject.Object);  // Tell the runner that it is the local player
        }
    }

    private void OnBeforeSpawned(NetworkRunner runner, NetworkObject obj)
    {
        spawnedKarts[spawnedKartIndex] = obj.gameObject;
        obj.GetComponentInChildren<KartInitializer>().Initialize(spawnedKartIndex + 1);
        spawnedKartIndex++;
    }
}
