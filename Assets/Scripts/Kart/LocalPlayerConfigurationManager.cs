using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Manages all of the local players in the game
// https://www.youtube.com/watch?v=_5pOiYHJgl0&t=3231s
// https://www.youtube.com/redirect?event=video_description&redir_token=QUlxb01IYXZiVXppTEtBMEtueFVtZ3Nac2lnNnxBQnJ0NjltenJxSFE2MlNHSmlBbnhJLUEtRXQ3cTcxMHFfWjNPY2ZFVGlnc0xxeGYyS20zY2RpVjhJejh5UkU5ZEx5NjZ6MmtPMkl0eFNvaDVWOHJzLWwtTHZGM0daRVlJTUpJ&q=https%3A%2F%2Fgithub.com%2FSrfigie%2FUnity_NewInputSystem_LocalCo-op&v=_5pOiYHJgl0

public class LocalPlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;
    [SerializeField] private int MaxPlayers = 4;

    public static LocalPlayerConfigurationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
        }

    }

    public void HandlePlayerJoin(PlayerInput input)
    {
        // A 'Player Configuration' prefab has just been spawned when the input system detected a new device

        input.transform.SetParent(transform);  // Parent that configuration object to the manager so it is not destroyed on load
        input.transform.name = "Player " + (input.playerIndex + 1) + " Configuration";

        if (!playerConfigs.Any(p => p.PlayerIndex == input.playerIndex))  // Checks to see if this player has been added already
        {
            Debug.Log("Player " + (input.playerIndex + 1) + " Joined");
            playerConfigs.Add(new PlayerConfiguration(input));  // Add that player configuration object to the list
        }
    }

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    //public void SetPlayerColor(int index, Material color)
    //{
    //    playerConfigs[index].playerMaterial = color;
    //}

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].isReady = true;
        if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.isReady == true))
        {
            //SceneManager.LoadScene("SampleScene");
            Debug.Log("All players ready");
        }
    }
}

public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput input)
    {
        PlayerIndex = input.playerIndex;
        Input = input;
    }

    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool isReady { get; set; }
    //public Material playerMaterial { get; set; }
}