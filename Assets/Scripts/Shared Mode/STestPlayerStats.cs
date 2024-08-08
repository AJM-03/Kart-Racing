using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Fusion.NetworkBehaviour;

public class STestPlayerStats : NetworkBehaviour
{
    public static STestPlayerStats Instance;

    [Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_32> PlayerName {  get; set; }

    [Networked, OnChangedRender(nameof(OnHatChanged))] public int hatIndex { get; set; }

    [Networked, OnChangedRender(nameof(OnHealthChanged))] public float health { get; set; }

    [SerializeField] TextMeshPro playerNameLabel;

    private GameObject currentHat;

    [SerializeField] private Transform playerHead;

    [SerializeField] private Image healthBar;


    private void Start()
    {
        if (this.HasStateAuthority)
        {
            if (Instance == null) Instance = this;
            PlayerName = STestFusionManager.Instance.playerName;
        }
    }

    public override void Spawned()
    {
        OnNameChanged();
        OnHatChanged();
    }

    public void HurtMe()
    {
        health -= 10;
    }

    public void OnNameChanged()
    {
        transform.root.gameObject.name = PlayerName.ToString();
        playerNameLabel.text = PlayerName.ToString();
    }

    public void OnHealthChanged()
    {
        healthBar.transform.localScale = new Vector3(health / 100, 1, 1);
    }

    public void OnHatChanged()
    {
        GameObject hat = STestHats.hats[hatIndex];

        if (currentHat != null) Destroy(currentHat);

        GameObject newHat = GameObject.Instantiate(hat);
        newHat.transform.parent = playerHead;
        newHat.transform.localPosition = Vector3.zero;
        newHat.transform.localRotation = Quaternion.identity;
        newHat.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newHat.GetComponent<BoxCollider>().enabled = false;
        currentHat = newHat;

        HurtMe();
    }
}
