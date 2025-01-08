using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ElementSelectionManager : NetworkBehaviour
{
    public event Action<Card.CardElement> OnElementButtonClicked;

    [SerializeField] private Button _waterButton;
    [SerializeField] private Button _electricityButton;
    [SerializeField] private Button _fireButton;

    [Rpc(SendTo.Server)]
    private void FireElementButtonClickedRpc(Card.CardElement element)
    {
        OnElementButtonClicked?.Invoke(element);
    }

    public override void OnNetworkSpawn()
    {
        _waterButton.onClick.AddListener(() => FireElementButtonClickedRpc(Card.CardElement.Water));
        _electricityButton.onClick.AddListener(() => FireElementButtonClickedRpc(Card.CardElement.Electricity));
        _fireButton.onClick.AddListener(() => FireElementButtonClickedRpc(Card.CardElement.Fire));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void SetButtonsActiveRpc(bool active, RpcParams rpcParams)
    {
        _waterButton.gameObject.SetActive(active);
        _electricityButton.gameObject.SetActive(active);
        _fireButton.gameObject.SetActive(active);
    }
}
