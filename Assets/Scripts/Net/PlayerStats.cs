using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Fusion.NetworkBehaviour;

public class PlayerStats : NetworkBehaviour
{
    public static PlayerStats Instance;

    [Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_32> PlayerName {  get; set; }

    [SerializeField] TextMeshPro playerNameLabel;

 
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
    }

    public void OnNameChanged()
    {
        transform.root.gameObject.name = PlayerName.ToString();
        playerNameLabel.text = PlayerName.ToString();
    }
}
