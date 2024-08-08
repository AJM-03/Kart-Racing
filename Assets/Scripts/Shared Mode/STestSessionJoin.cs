using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class STestSessionJoin : MonoBehaviour
{
    public static STestSessionJoin Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject sessionSearchCanvas;
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button joinSessionButton;

    private string password;

    public void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OpenJoinMenu(string sessionName, string sessionPassword)
    {
        sessionNameText.text = sessionName;
        password = sessionPassword;
        panel.SetActive(true);
        sessionSearchCanvas.SetActive(false);
        passwordInput.text = "";
    }

    public void JoinSession()
    {
        STestFusionManager.Instance.ConnectToSession(sessionNameText.text);
        panel.SetActive(false);
    }

    public void ChangePassword()
    {
        if (passwordInput.text == password)
            joinSessionButton.interactable = true;

        else
            joinSessionButton.interactable = false;
    }

    public void Back()
    {
        sessionSearchCanvas.SetActive(true);
        panel.SetActive(false);
    }
}
