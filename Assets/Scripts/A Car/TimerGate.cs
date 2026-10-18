using UnityEngine;

public class TimerGate : MonoBehaviour
{
    public TestingTimer timer;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Kart"))
            timer.ToggleTimer();
    }
}
