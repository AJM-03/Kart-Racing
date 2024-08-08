using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class STestChatSystem : NetworkBehaviour
{
    [Header("GameObjects")]
    public GameObject chatEntryCanvas;
    public TMP_InputField chatEntryInput;
    public GameObject chatDisplayCanvas;
    public TextMeshProUGUI chatBody;
    [HideInInspector] public static TextMeshProUGUI myChatBody;
    [HideInInspector] public static GameObject myChatDisplay;

    [Header("Actions")]
    public InputActionReference startChat;
    public InputActionReference sendChat;

    [Header("Networked")]
    private bool placeholder;
    [HideInInspector][Networked, OnChangedRender(nameof(LastPublicChatChanged))] public NetworkString<_256> LastPublicChat { get; set; }
    [HideInInspector][Networked, OnChangedRender(nameof(LastPrivateChatChanged))] public NetworkString<_256> LastPrivateChat { get; set; }

    private string thisPlayersName;


    private void Start()
    {
        if (this.HasStateAuthority)
        {
            startChat.action.performed += StartChat;
            sendChat.action.performed += SendChat;
            myChatBody = chatBody;
            myChatDisplay = chatDisplayCanvas;
        }

        thisPlayersName = transform.root.GetComponent<STestPlayerStats>().PlayerName.ToString();
    }


    public void LastPublicChatChanged()
    {
        myChatDisplay.SetActive(true);
        myChatBody.text += "\n [" + thisPlayersName + "] " + LastPublicChat.ToString();
    }


    public void LastPrivateChatChanged()
    {

    }


    private void StartChat(InputAction.CallbackContext obj)
    {
        chatEntryCanvas.SetActive(true);
        chatEntryInput.Select();
    }


    private void SendChat(InputAction.CallbackContext obj)
    {
        LastPublicChat = chatEntryInput.text;
        chatEntryInput.text = string.Empty;
        chatEntryCanvas.SetActive(false);
    }
}
