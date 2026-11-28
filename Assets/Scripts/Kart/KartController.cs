using Cinemachine;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [Header("Kart Info")]
    public int racerIndex;  // A unique ID for each racer in the game
    public int playerNumber;  // Represents what is controlling this racer 1-4=Local Player, 5=AI, 6=Network Player
    public bool canMove, canTransform;  // Can be used to limit the player's abilities
    public KartEnums.CurrentTerrainState currentTerrainState;  // The type of terrain the vehicle is currently on (Land, Sea, Sky)
    public KartInputData currentInput;  // The current input values for this frame


    [Header("References")]
    [HideInInspector] public Rigidbody rb;  // The kart's rigidBody component
    [SerializeField] private CameraController cameraController;  // The camera logic for the kart
    [SerializeField] private KartWallInteractions wallInteractions;  // Detects and controls the kart's interactions with walls
    [SerializeField] private KartInput inputSource;  // The script that the input should be coming from


    [Header("Kart Forms")]
    public KartEnums.KartForm currentForm;  // The form that the kart is currently in
    public KartForm currentFormScript;  // The script for the kart's current form
    public Car car;  // The script for the car form






    private void Update()
    {
        if (!IsfullyInitialized()) return;
        currentInput = inputSource.GetInput();
        currentFormScript.UpdateKart();
        cameraController.UpdateCamera();
    }



    private void FixedUpdate()
    {
        if (!IsfullyInitialized()) return;
        currentFormScript.CalculateKartVelocity();
        wallInteractions.WallDetection();
        currentFormScript.FixedUpdateKart();
    }



    public void ChangeForm(KartEnums.KartForm targetForm, bool instant)  // Changes the current form of the kart
    {
        if (targetForm == currentForm && !instant) return;
        if (!canTransform) return;

        if (currentFormScript) currentFormScript.TransformOut(targetForm);
        KartEnums.KartForm oldForm = currentForm;

        currentForm = targetForm;
        if (targetForm == KartEnums.KartForm.Car) currentFormScript = car;
        if (targetForm == KartEnums.KartForm.Boat) currentFormScript = car;
        if (targetForm == KartEnums.KartForm.Plane) currentFormScript = car;

        currentFormScript.TransformInto(oldForm);
    }





    // Getters
    public bool IsfullyInitialized() { return GetComponent<KartNetworkController>().spawned && GetComponent<KartInitializer>().initialized; }
    public float GetVelocityRatio() { return currentFormScript.carVelocityRatio; }  // Returns a float between -1 and 1 showing how fast the kart is moving in relation to it's top speed
    public Vector3 GetVelocity() { return currentFormScript.currentCarLocalVelocity; }  // Returns a vector showing the kart's current velocity
    public bool IsGrounded() { return currentTerrainState == KartEnums.CurrentTerrainState.Land; }  // Returns true if the kart is currently grounded
    public bool IsInWater() { return currentTerrainState == KartEnums.CurrentTerrainState.Sea; }  // Returns true if the kart is currently in water
    public float GetAccelDirection() { return currentInput.Accelerate ? 1 : (currentInput.Brake ? -1 : 0); }  // Returns a float that shows 1 for an accel input and -1 if braking instead
    public bool IsBraking() { return GetAccelDirection() < 0 && GetVelocityRatio() > 0; }  // Returns true if the kart is currently braking
    public bool IsDrifting() { return currentFormScript.isDrifting; }  // Returns true if the kart is currently drifting
    public bool IsBrakeDrifting() { return IsDrifting() && currentInput.Brake; }  // Returns true if the kart is currently brake drifting
    public float GetDriftDirection() { return currentFormScript.driftDirection; }  // Returns the direction that the kart is drifting in, (-1 for left, 1 for right)
    public float GetDriftControl() { return currentFormScript.driftControl; }  // Returns a float showing how tightly the kart is currently drifting
    public bool IsLocalPlayer() {  return playerNumber >= 1 &&  playerNumber <= 4; }  // Returns true if this kart belongs to a real local player
}
