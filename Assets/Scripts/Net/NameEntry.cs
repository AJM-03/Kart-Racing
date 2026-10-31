using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameEntry : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;

    public void SubmitName()
    {
        FusionManager.Instance.ConnectToLobby(inputField.text);
        inputField.gameObject.SetActive(false);
    }

    public void ActivateButton()
    {
        if (inputField.text != "")
            submitButton.interactable = true;

        else            
            submitButton.interactable = false;
    }
}
