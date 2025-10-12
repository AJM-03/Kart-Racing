using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Fusion.NetworkBehaviour;

public class HTestPlayerStats : NetworkBehaviour
{
    public static HTestPlayerStats Instance;
    [Networked] public NetworkString<_32> PlayerName { get; set; }

    [Networked] public int hatIndex { get; set; }

    [Networked] public float health { get; set; }

    //[Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_32> PlayerName {  get; set; }

    //[Networked, OnChangedRender(nameof(OnHatChanged))] public int hatIndex { get; set; }

    //[Networked, OnChangedRender(nameof(OnHealthChanged))] public float health { get; set; }a

    [SerializeField] TextMeshPro playerNameLabel;

    private GameObject currentHat;

    [SerializeField] private Transform playerHead;

    [SerializeField] private Image healthBar;


    private void Start()
    {
        if (this.HasInputAuthority)
        {
            if (Instance == null) Instance = this;
        }
    }

    public override void Spawned()
    {
        if (this.HasInputAuthority)
        {
            PlayerName = HTestSpawner.Instance.playerName;
        }

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
        healthBar.transform.localScale = new Vector3(Mathf.Clamp(health / 100, 0, 1), 1, 1);
    }

    public void OnHatChanged()
    {
        if (STestHats.hats == null || STestHats.hats.Count == 0) return;
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
