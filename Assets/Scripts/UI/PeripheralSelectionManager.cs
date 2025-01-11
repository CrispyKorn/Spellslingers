using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PeripheralSelectionManager : NetworkBehaviour
{
    public event Action<bool> OnPeripheralTypeSelected;

    [SerializeField] private Button _offenceSelection;
    [SerializeField] private Button _defenceSelection;

    [Rpc(SendTo.Server)]
    private void FirePeripheralTypeSelectedRpc(bool offenceSelected)
    {
        OnPeripheralTypeSelected?.Invoke(offenceSelected);
    }

    public override void OnNetworkSpawn()
    {
        _offenceSelection.onClick.AddListener(() => FirePeripheralTypeSelectedRpc(true));
        _defenceSelection.onClick.AddListener(() => FirePeripheralTypeSelectedRpc(false));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void SetButtonsActiveRpc(bool active, RpcParams rpcParams)
    {
        _offenceSelection.gameObject.SetActive(active);
        _defenceSelection.gameObject.SetActive(active);
    }
}
