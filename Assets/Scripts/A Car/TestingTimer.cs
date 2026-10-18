using System.Collections;
using TMPro;
using UnityEngine;

public class TestingTimer : MonoBehaviour
{
    private TMP_Text text;
    private float timer;
    private bool isRunning;
    public float hideTime;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        text.enabled = false;
        isRunning = false;
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
        }

        float minutes = Mathf.FloorToInt(timer / 60);
        float seconds = Mathf.FloorToInt(timer % 60);
        float milliseconds = Mathf.FloorToInt((timer -seconds) * 100);
        text.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);

        if (Input.GetButtonDown("Submit")) ToggleTimer();
    }

    public void ToggleTimer()
    {
        StopAllCoroutines();
        isRunning = !isRunning;
        text.enabled = true;
        if (isRunning) timer = 0;
        else StartCoroutine(HideTimer());
    }

    private IEnumerator HideTimer()
    {
        yield return new WaitForSeconds(hideTime);
        text.enabled = false;
    }
}
