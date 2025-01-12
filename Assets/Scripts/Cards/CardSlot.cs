using UnityEngine;
using Unity.Netcode;

public class CardSlot : NetworkBehaviour
{
    [System.Serializable]
    public enum SlotType
    {
        Core,
        Peripheral,
        Utility
    }

    public bool IsUsable { get => _isUsable; set => _isUsable = value; }
    public bool HasCard { get => _hasCard; }
    public Bounds Bounds { get => _boxCollider.bounds; }
    public GameObject Card { get => _heldCard; }
    public SlotType Type { get => _type; }

    [SerializeField] SlotType _type;

    [Header("Read Only")]
    [BeginReadOnlyGroup]
    [SerializeField] private bool _isUsable = true;
    [SerializeField] private bool _hasCard;
    [EndReadOnlyGroup]

    private BoxCollider2D _boxCollider;
    private GameObject _heldCard;

    /// <summary>
    /// Places the held card.
    /// </summary>
    private void PlaceCard()
    {
        var heldPlayCard = _heldCard.GetComponent<PlayCard>();

        _hasCard = true;
        heldPlayCard.SetCardTransformRpc(transform.position, transform.rotation);
        heldPlayCard.ResetTransformRpc();
        heldPlayCard.Placed = true;
        heldPlayCard.PlacedCardSlot = this;
    }

    public override void OnNetworkSpawn()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Attempts to place the given card into the slot.
    /// </summary>
    /// <param name="_card">The card to place.</param>
    /// <returns>Whether placement was successful.</returns>
    public bool TryPlaceCard(GameObject _card, bool ignoreUsable = false)
    {
        if (_hasCard || (!ignoreUsable && !_isUsable)) return false;

        _heldCard = _card;

        PlaceCard();
        return true;
    }

    /// <summary>
    /// Removes the card from the slot.
    /// </summary>
    /// <returns>The removed card.</returns>
    public GameObject TakeCard()
    {
        if (_heldCard == null) return null;

        GameObject tempCard = _heldCard;
        var heldPlayCard = _heldCard.GetComponent<PlayCard>();
        heldPlayCard.Placed = false;
        heldPlayCard.PlacedCardSlot = null;
        _heldCard = null;
        _hasCard = false;

        return tempCard;
    }
}
