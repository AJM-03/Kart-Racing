using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

// Will send the local player's input to the kart

public class PlayerInputHandler : KartInput
{
    private PlayerConfiguration playerConfig;
    private PlayerControls controls;
    public bool accelerateInput; 
    public bool brakeInput;  
    public float steerInput; 
    public bool driftInput; 
    public bool itemInput; 
    public bool transformInput; 
    public bool pauseInput;

    public void Awake()
    {
        Init();
        controls = new PlayerControls();
    }


    public void InitializePlayer(PlayerConfiguration config)
    {
        playerConfig = config;
        Debug.Log("Setting up controls for player " + (config.PlayerIndex + 1));
        config.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(CallbackContext obj)
    {
        if (obj.action.name == controls.Driving.Accelerate.name)
        {
            OnAccelerate(obj);
        }
        if (obj.action.name == controls.Driving.Brake.name)
        {
            OnBrake(obj);
        }
        if (obj.action.name == controls.Driving.Steer.name)
        {
            OnSteer(obj);
        }
        if (obj.action.name == controls.Driving.Drift.name)
        {
            OnDrift(obj);
        }
        if (obj.action.name == controls.Driving.Item.name)
        {
            OnItem(obj);
        }
        if (obj.action.name == controls.Driving.Transform.name)
        {
            OnTransform(obj);
        }
        if (obj.action.name == controls.Driving.Pause.name)
        {
            OnPause(obj);
        }
    }


    public void OnAccelerate(CallbackContext context)
    {
        accelerateInput = context.performed;
    }
    public void OnBrake(CallbackContext context)
    {
        brakeInput = context.performed;
    }
    public void OnSteer(CallbackContext context)
    {
        steerInput = context.ReadValue<float>();
    }
    public void OnDrift(CallbackContext context)
    {
        driftInput = context.performed;
    }
    public void OnItem(CallbackContext context)
    {
        itemInput = context.performed;
    }
    public void OnTransform(CallbackContext context)
    {
        transformInput = context.performed;
    }
    public void OnPause(CallbackContext context)
    {
        pauseInput = context.performed;
    }


    public override KartInputData GetInput()
    {
        //var playerActions = _playerActionMap.Driving;


        //data = new KartInputData(
        //    playerActions.Accelerate.IsPressed(),
        //    playerActions.Brake.IsPressed(),
        //    playerActions.Steer.ReadValue<float>(),
        //    playerActions.Drift.IsPressed(),
        //    playerActions.Item.IsPressed(),
        //    playerActions.Transform.IsPressed(),
        //    playerActions.Pause.IsPressed()
        //    );
        //Debug.Log("1 - " + data.Accelerate);

        //base.GetInput();

        //Debug.Log("2 - " + data.Accelerate);
        //return data;


        data = new KartInputData(
            accelerateInput,
            brakeInput,
            steerInput,
            driftInput,
            itemInput,
            transformInput,
            pauseInput
            );

        Debug.Log("1 - " + data.Accelerate);

        base.GetInput();

        Debug.Log("2 - " + data.Accelerate);

        return data;
    }
}
