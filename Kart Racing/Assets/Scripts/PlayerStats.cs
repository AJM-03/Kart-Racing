using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public class PlayerStats : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnNameChanged))] public NetworkString<_32> PlayerName {  get; set; }

    [SerializeField] TextMeshPro playerNameLabel;

    private void Start()
    {
        if (this.HasStateAuthority)
            PlayerName = FusionManager.Instance.playerName;
    }

    public override void Spawned()
    {
        OnNameChanged();
    }

    public void OnNameChanged()
    {
        playerNameLabel.text = PlayerName.ToString();
    } 
}
