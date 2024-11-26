using UnityEngine;
using Unity.Netcode;

public class CardSlot : NetworkBehaviour
{
    public bool IsUsable { get => n_isUsable.Value; set => n_isUsable.Value = value; }
    public bool HasCard { get => n_hasCard.Value; }
    public Bounds Bounds { get => _boxCollider.bounds; }
    public GameObject Card { get => _heldCard; }
    public bool IsUtilitySlot { get => _isUtilitySlot; }

    [SerializeField] bool _isUtilitySlot = false;

    private NetworkVariable<bool> n_isUsable = new(true);
    private NetworkVariable<bool> n_hasCard = new(false);

    private BoxCollider2D _boxCollider;
    private GameObject _heldCard;

    [ClientRpc]
    private void UpdateCardClientRpc(ulong cardNetworkObjectId)
    {
        _heldCard = GetNetworkObject(cardNetworkObjectId).gameObject;
        _heldCard.transform.position = transform.position;
        _heldCard.transform.rotation = transform.rotation;
        if (IsHost) _heldCard.GetComponent<PlayCard>().Moveable = false;
    }

    public override void OnNetworkSpawn()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    public bool TryPlaceCard(GameObject _card)
    {
        if (n_hasCard.Value || !n_isUsable.Value) return false;

        _heldCard = _card;
        PlaceCardServerRpc(_heldCard.GetComponent<NetworkObject>().NetworkObjectId);
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaceCardServerRpc(ulong cardNetworkObjectId)
    {
        n_hasCard.Value = true;
        UpdateCardClientRpc(cardNetworkObjectId);
    }

    public GameObject TakeCard()
    {
        GameObject tempCard = _heldCard;
        _heldCard = null;
        n_hasCard.Value = false;

        return tempCard;
    }
}
