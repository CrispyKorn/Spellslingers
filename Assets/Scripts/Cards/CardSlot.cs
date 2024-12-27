using UnityEngine;
using Unity.Netcode;

public class CardSlot : NetworkBehaviour
{
    [SerializeField] public bool IsUsable { get => n_isUsable.Value; set => n_isUsable.Value = value; }
    public bool HasCard { get => n_hasCard.Value; }
    public Bounds Bounds { get => _boxCollider.bounds; }
    public GameObject Card { get => _heldCard; }
    public bool IsUtilitySlot { get => _isUtilitySlot; }

    [SerializeField] bool _isUtilitySlot = false;

    [Header("Read Only")]
    [BeginReadOnlyGroup]
    [SerializeField] private NetworkVariable<bool> n_isUsable = new(true);
    [SerializeField] private NetworkVariable<bool> n_hasCard = new(false);
    [EndReadOnlyGroup]

    private BoxCollider2D _boxCollider;
    private GameObject _heldCard;

    /// <summary>
    /// Places the held card.
    /// </summary>
    private void PlaceCard()
    {
        n_hasCard.Value = true;
        _heldCard.transform.position = transform.position;
        _heldCard.transform.rotation = transform.rotation;
        _heldCard.GetComponent<PlayCard>().Placed = true;
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
    public bool TryPlaceCard(GameObject _card)
    {
        if (n_hasCard.Value || !n_isUsable.Value) return false;

        _heldCard = _card;
        _heldCard.GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.ServerClientId);
        PlaceCard();
        return true;
    }

    /// <summary>
    /// Removes the card from the slot.
    /// </summary>
    /// <returns>The removed card.</returns>
    public GameObject TakeCard()
    {
        GameObject tempCard = _heldCard;
        _heldCard = null;
        n_hasCard.Value = false;

        return tempCard;
    }
}
