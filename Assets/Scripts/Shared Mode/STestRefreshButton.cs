using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class STestRefreshButton : MonoBehaviour
{
    private Button refreshButton;

    private void Awake()
    {
        refreshButton = GetComponent<Button>();
        refreshButton.onClick.AddListener(Refresh);
        refreshButton.interactable = true;
    }

    private void Refresh()
    {
        StartCoroutine(RefreshWait());
    }

    private IEnumerator RefreshWait()
    {
        refreshButton.interactable = false;

        STestFusionManager.Instance.RefreshSessionListUI();

        yield return new WaitForSeconds(3);

        refreshButton.interactable = true;
    }
}
