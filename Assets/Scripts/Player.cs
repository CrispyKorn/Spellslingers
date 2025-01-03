using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public event Action<Player, GameObject> OnPlaceCard;
    public event Action<Player, PlayCard> OnCardSelected;

    public int Health { get => n_health.Value; set => n_health.Value = value; }
    public NetworkVariable<int> N_Health { get => n_health; }
    public Deck Hand { get => _hand; set => _hand = value; }

    [SerializeField] private LayerMask _cardCollisionMask;
    [SerializeField] private LayerMask _cardSlotCollisionMask;

    private NetworkVariable<int> n_health = new(30);

    private Deck _hand;
    private PlayCard _selectedCard = null;
    private SpriteRenderer _selectedCardZoom;

    private void Awake()
    {
        _hand = new Deck();
    }

    private void OnEnable()
    {
        SubToInputEvents();
    }

    private void OnDisable()
    {
        if (!IsSpawned || !IsOwner) return;

        Locator.Instance.InputManager.OnSelect -= Select;
        Locator.Instance.InputManager.OnSelect_Ended -= TryPlaceCardRpc;
    }

    /// <summary>
    /// Subscribes to the relevant input events when available.
    /// </summary>
    private async void SubToInputEvents()
    {
        if (!IsSpawned || !IsOwner) return;

        while (Locator.Instance.InputManager == null) await Awaitable.NextFrameAsync();

        Locator.Instance.InputManager.OnSelect += Select;
        Locator.Instance.InputManager.OnSelect_Ended += TryPlaceCardRpc;
    }

    /// <summary>
    /// Tells the server to select the card under the mouse.
    /// </summary>
    private void Select()
    {
        TrySelectCardRpc(Locator.Instance.InputManager.MousePos, OwnerClientId);
    }

    /// <summary>
    /// Attempts to select a card under the cursor position for the given player. Runs on the server.
    /// </summary>
    /// <param name="mousePos">The position to check for a card.</param>
    /// <param name="invokingClientId">The client ID of the player selecting.</param>
    [Rpc(SendTo.Server)]
    private void TrySelectCardRpc(Vector2 mousePos, ulong invokingClientId)
    {
        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(_cardCollisionMask);
        List<Collider2D> collisions = new();
        int numOfOverlaps = Physics2D.OverlapPoint(mousePos, contactFilter, collisions);

        // End early when no cards are hit
        if (numOfOverlaps == 0)
        {
            UpdateSelectedCardRpc(0);
            DisableSelectedCardZoomRpc();
            return;
        }

        // Only select cards that the player is allowed to see
        List<Collider2D> disallowedCardCollisions  = new();
        foreach (Collider2D collision in collisions)
        {
            var playCard = collision.GetComponent<PlayCard>();
            bool isNotCardOwner = playCard.OwnerClientId != invokingClientId;
            bool cardIsFaceDown = !playCard.IsFaceUp;
            bool cardIsInHand = !playCard.Placed;

            // Card can only be selected by its owner, or when it is placed and face-up
            if (isNotCardOwner && (cardIsInHand || cardIsFaceDown)) disallowedCardCollisions.Add(collision);
        }

        // Find the topmost card if multiple were hit
        var topCard = collisions[0].GetComponent<PlayCard>();
        if (collisions.Count > 1)
        {
            foreach (Collider2D collision in collisions)
            {
                if (collision.transform.position.z < topCard.transform.position.z) topCard = collision.GetComponent<PlayCard>();
            }
        }

        PlayCard selectedCard = topCard;

        // Show zoomed card when allowed
        if (!disallowedCardCollisions.Contains(selectedCard.BoxCollider))
        {
            UpdateSelectedCardRpc(selectedCard.NetworkObjectId);
            SetSelectedCardZoomRpc();
            _selectedCard.OnSelected();
        }

        OnCardSelected?.Invoke(this, selectedCard);
    }

    /// <summary>
    /// Locally updates _selectedCard using the given network ID for everyone on the network.
    /// </summary>
    /// <param name="selectedCardNetworkId">The network object ID of the selected card.</param>
    [Rpc(SendTo.Everyone)]
    private void UpdateSelectedCardRpc(ulong selectedCardNetworkId)
    {
        if (selectedCardNetworkId == 0) _selectedCard = null;
        else _selectedCard = GetNetworkObject(selectedCardNetworkId).GetComponent<PlayCard>();
    }

    /// <summary>
    /// Disables the zoomed card preview for the local player.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void DisableSelectedCardZoomRpc()
    {
        _selectedCardZoom.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates and activates the zoomed card preview for the local player.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void SetSelectedCardZoomRpc()
    {
        _selectedCardZoom.sprite = _selectedCard.CardData.FrontImg;
        _selectedCardZoom.gameObject.SetActive(true);
    }

    /// <summary>
    /// Attempts to place the held card. Runs on the server.
    /// </summary>
    [Rpc(SendTo.Server)]
    private void TryPlaceCardRpc()
    {
        // Early exit when placing impossible
        if (_selectedCard == null || _selectedCard.Placed) return;

        var placed = false;

        // Check for an overlap between the card and a card slot
        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(_cardSlotCollisionMask);
        List<Collider2D> collisions = new();
        int numOfOverlaps = _selectedCard.BoxCollider.Overlap(contactFilter, collisions);

        if (numOfOverlaps > 0) // Successful card slot overlap
        {
            // Find the closest card slot
            var cardSlot = collisions[0].GetComponent<CardSlot>();
            float minDist = Vector2.Distance(_selectedCard.transform.position, cardSlot.transform.position);
            var dist = 0f;

            for (int i = 1; i < collisions.Count; i++)
            {
                dist = Vector2.Distance(_selectedCard.transform.position, collisions[i].transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    cardSlot = collisions[i].GetComponent<CardSlot>();
                }
            }
            
            // Check for invalid card placements
            bool isUtility = _selectedCard.CardData.Type == ICard.CardType.Utility;
            bool isValidCard = Locator.Instance.PlayManager.CheckValidCard(_selectedCard.CardData);
            bool canPlace = cardSlot.IsUtilitySlot ? isUtility : isValidCard;

            if (canPlace)
            {
                if (cardSlot.TryPlaceCard(_selectedCard.gameObject))
                {
                    // Card placement allowed, place the card!
                    _selectedCard.SetIsBeingDraggedRpc(false);
                    OnPlaceCard?.Invoke(this, _selectedCard.gameObject);
                    placed = true;
                }
            }
        }

        if (!placed) // Placing into card slot failed, reset card
        {
            _selectedCard.SetIsBeingDraggedRpc(false);
            _selectedCard.ResetPosRpc();
        }
    }

    public override async void OnNetworkSpawn()
    {
        while (Locator.Instance.PlayManager == null) await Awaitable.NextFrameAsync();
        _selectedCardZoom = Locator.Instance.PlayManager.SelectedCard;

        SubToInputEvents();        
    }
}
