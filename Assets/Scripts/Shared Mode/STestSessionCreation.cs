using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class STestSessionCreation : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject sessionSearchCanvas;
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text sessionPrivacyText;
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button createSessionButton;

    public void CreateSession()
    {
        STestFusionManager.Instance.CreateSession(sessionNameInput.text, passwordInput.text, (int) playerCountSlider.value);
        canvas.SetActive(false);
    }

    public void ChangeSessionName()
    {
        if (sessionNameInput.text != "")
            createSessionButton.interactable = true;

        else
            createSessionButton.interactable = false;
    }

    public void ChangePrivacy()
    {
        if (passwordInput.text != "")
            sessionPrivacyText.text = "Privacy: Private";

        else
            sessionPrivacyText.text = "Privacy: Public";
    }

    public void ChangePlayerCount()
    {
        playerCountText.text = "Player Limit: " + playerCountSlider.value;
    }

    public void Back()
    {
        sessionSearchCanvas.SetActive(true);
        canvas.SetActive(false);
    }
}
