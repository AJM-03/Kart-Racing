using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class STestSessionEntry : MonoBehaviour
{
    public TextMeshProUGUI sessionName;
    public string sessionPassword;
    public TextMeshProUGUI playerCount;
    public TextMeshProUGUI privacyText;
    public Button joinButton;

    private void Awake()
    {
        joinButton.onClick.AddListener(JoinSession);
    }

    private void Start()
    {
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }

    private void JoinSession()
    {
        if (privacyText.text == "Public")
            STestFusionManager.Instance.ConnectToSession(sessionName.text);
        else
        {
            STestSessionJoin.Instance.OpenJoinMenu(sessionName.text, sessionPassword);
        }
    }
}
