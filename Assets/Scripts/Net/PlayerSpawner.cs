using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour
{
    public GameObject playerPrefab;
    public Vector3 spawnPoint;

    private void Awake()
    {
        var runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (!runner.IsRunning) return;
            
        runner.AddGlobal(this);

        NetworkSpawnOp playerObject = Runner.SpawnAsync(playerPrefab, spawnPoint, Quaternion.identity);  // Spawn the player object
        Runner.SetPlayerObject(Runner.LocalPlayer, playerObject.Object);  // Tell the runner that it is the local player
    }
}
