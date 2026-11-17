using UnityEngine;
using static Unity.Collections.Unicode;

// Will send the local player's input to the kart

public class PlayerInput : KartInput
{
    private PlayerControls _playerActionMap;

    public void Awake()
    {
        Init();
        EnableActionMap();
    }

    public void OnEnable() { EnableActionMap(); }

    public void OnDisable() { DisableActionMap(); }

    private void EnableActionMap()
    {
        if (_playerActionMap == null) _playerActionMap = new PlayerControls();
        _playerActionMap.Driving.Enable();
    }

    private void DisableActionMap()
    {
        if (_playerActionMap != null) _playerActionMap.Driving.Disable();
    }


    public override KartInputData GetInput()
    {
        var playerActions = _playerActionMap.Driving;


        data = new KartInputData(
            playerActions.Accelerate.IsPressed(),
            playerActions.Brake.IsPressed(),
            playerActions.Steer.ReadValue<float>(),
            playerActions.Drift.IsPressed(),
            playerActions.Item.IsPressed(),
            playerActions.Transform.IsPressed(),
            playerActions.Pause.IsPressed()
            );
        Debug.Log("1 - " + data.Accelerate);

        base.GetInput();

        Debug.Log("2 - " + data.Accelerate);
        return data;
    }
}
