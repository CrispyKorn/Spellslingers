using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CardSlot : NetworkBehaviour
{
    BoxCollider2D boxCollider;
    NetworkVariable<bool> hasCard = new NetworkVariable<bool>(false);
    GameObject card;
    [SerializeField] bool isUtilitySlot = false;
    
    public NetworkVariable<bool> useable = new NetworkVariable<bool>(true);

    public bool HasCard { get { return hasCard.Value; } }
    public Bounds Bounds { get { return boxCollider.bounds; } }
    public GameObject Card { get { return card; } }
    public bool IsUtilitySlot { get { return isUtilitySlot; } }

    public override void OnNetworkSpawn()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public bool TryPlaceCard(GameObject _card)
    {
        if (hasCard.Value || !useable.Value) return false;
        card = _card;
        PlaceCardServerRpc(card.GetComponent<NetworkObject>().NetworkObjectId);
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaceCardServerRpc(ulong cardNetworkObjectId)
    {
        hasCard.Value = true;
        UpdateCardClientRpc(cardNetworkObjectId);
    }

    [ClientRpc]
    private void UpdateCardClientRpc(ulong cardNetworkObjectId)
    {
        card = GetNetworkObject(cardNetworkObjectId).gameObject;
        card.transform.position = transform.position;
        card.transform.rotation = transform.rotation;
        if (IsHost) card.GetComponent<PlayCard>().moveable.Value = false;
    }

    public GameObject TakeCard()
    {
        GameObject tempCard = card;
        card = null;
        hasCard.Value = false;

        return tempCard;
    }
}
