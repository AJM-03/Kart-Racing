using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public class PlayerStats : NetworkBehaviour
{
    public static PlayerStats Instance;

    [Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_32> PlayerName {  get; set; }

    [Networked, OnChangedRender(nameof(OnHatChanged))] public int hatIndex { get; set; }

    [SerializeField] TextMeshPro playerNameLabel;

    private GameObject currentHat;

    [SerializeField] private Transform playerHead;


    private void Start()
    {
        if (this.HasStateAuthority)
        {
            if (Instance == null) Instance = this;
            PlayerName = FusionManager.Instance.playerName;
        }
    }

    public override void Spawned()
    {
        OnNameChanged();
        OnHatChanged();
    }

    public void OnNameChanged()
    {
        playerNameLabel.text = PlayerName.ToString();
    }

    public void OnHatChanged()
    {
        GameObject hat = Hats.hats[hatIndex];

        if (currentHat != null) Destroy(currentHat);

        GameObject newHat = GameObject.Instantiate(hat);
        newHat.transform.parent = playerHead;
        newHat.transform.localPosition = Vector3.zero;
        newHat.transform.localRotation = Quaternion.identity;
        newHat.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newHat.GetComponent<BoxCollider>().enabled = false;
        currentHat = newHat;
    }
}
