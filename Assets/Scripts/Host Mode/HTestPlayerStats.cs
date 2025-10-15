using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static Fusion.NetworkBehaviour;

public class HTestPlayerStats : NetworkBehaviour
{
    public static HTestPlayerStats localPlayer;
    private ChangeDetector changeDetector;
    [Networked] public NetworkString<_32> PlayerName { get; set; }

    public int localHatIndex;
    [Networked] public int hatIndex { get; set; }

    [Networked] public float health { get; set; }

    [SerializeField] TextMeshPro playerNameLabel;

    private GameObject currentHat;

    [SerializeField] private Transform playerHead;

    [SerializeField] private Image healthBar;


    private void Start()
    {
        if (HasInputAuthority)
        {
            if (localPlayer == null) localPlayer = this;
        }
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasInputAuthority)
        {
            RPC_SendNameChange(HTestSpawner.Instance.playerName);
        }

        OnNameChanged();
        localHatIndex = 0;
        OnHatChanged();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out HTestNetworkInputData data) == false || !HasStateAuthority) return;
        if (hatIndex != data.hatIndex && data.hatIndex != 0) hatIndex = data.hatIndex;
    }

    public override void Render()
    {
        foreach(var change in changeDetector.DetectChanges(this, out var previous, out var current))
        {
            switch(change)
            {
                case nameof(PlayerName):
                    {
                        OnNameChanged();
                        break;
                    }
                case nameof(health):
                    {
                        OnHealthChanged();
                        break;
                    }
                case nameof(hatIndex):
                    {
                        OnHatChanged();
                        if (HasStateAuthority) HurtMe();
                        break;
                    }
            }
        }
    }

    public void HurtMe()
    {
        health -= 10;
    }

    public void OnHealthChanged()
    {
        Debug.Log("Health Changed");

        healthBar.transform.localScale = new Vector3(Mathf.Clamp(health / 100, 0, 1), 1, 1);
    }

    public void OnHatChanged()
    {
        if (STestHats.hats == null || STestHats.hats.Count == 0 || hatIndex == 0) return;
        GameObject hat = STestHats.hats[hatIndex - 1];

        if (currentHat != null) Destroy(currentHat);

        GameObject newHat = GameObject.Instantiate(hat);
        newHat.transform.parent = playerHead;
        newHat.transform.localPosition = Vector3.zero;
        newHat.transform.localRotation = Quaternion.identity;
        newHat.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newHat.GetComponent<BoxCollider>().enabled = false;
        currentHat = newHat;
        Debug.Log("Hat Changed");

        if (HasInputAuthority)
            localHatIndex = 0;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendNameChange(string message, RpcInfo info = default)
    {
        PlayerName = message;
    }

    public void OnNameChanged()
    {
        Debug.Log(PlayerName + "'s name Changed");

        transform.root.gameObject.name = PlayerName.ToString();
        playerNameLabel.text = PlayerName.ToString();
    }
}
