using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RefreshButton : MonoBehaviour
{
    private Button refreshButton;

    private void Awake()
    {
        refreshButton = GetComponent<Button>();
        refreshButton.onClick.AddListener(Refresh);
    }

    private void Refresh()
    {
        StartCoroutine(RefreshWait());
    }

    private IEnumerator RefreshWait()
    {
        refreshButton.interactable = false;

        FusionManager.Instance.RefreshSessionListUI();

        yield return new WaitForSeconds(3);

        refreshButton.interactable = true;
    }
}
