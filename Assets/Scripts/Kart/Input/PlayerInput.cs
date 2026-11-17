using UnityEngine;
using static Unity.Collections.Unicode;

// Will send the local player's input to the kart

public class PlayerInput : KartInput
{
    private PlayerControls _playerActionMap;

    public void Awake()
    {
        EnableActionMap();
    }

    private void EnableActionMap()
    {
        if (_playerActionMap == null) _playerActionMap = new PlayerControls();
        _playerActionMap.Movement.Enable();
    }


    public override KartInputData GetInput()
    {
        if (!HasInputAuthority) base.GetInput();

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

        base.GetInput();
        return data;
    }
}
