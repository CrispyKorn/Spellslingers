using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : NetworkBehaviour
{   
    /// <summary>
    /// Triggered when a card is placed. [placing player, placed card, slot placed into]
    /// </summary>
    public event Action<Player, GameObject, CardSlot> OnPlaceCard;
    /// <summary>
    /// Triggered when a card is selected. [selecting player, selected card]
    /// </summary>
    public event Action<Player, PlayCard> OnCardSelected;
    
    public bool PickupDisabled { get => _pickupDisabled; set => _pickupDisabled = value; }

    [SerializeField] private LayerMask _cardCollisionMask;
    [SerializeField] private LayerMask _cardSlotCollisionMask;

    private Player _player;
    private bool _pickupDisabled;
    private PlayCard _selectedCard = null;
    private Image _selectedCardZoom;

    private void Awake()
    {
        _player = GetComponent<Player>();
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

    #region Private
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

        // Find the closest card to cursor if multiple were hit
        PlayCard selectedCard = collisions[0].GetComponent<PlayCard>();
        float selectedCardDist = Mathf.Infinity;

        if (collisions.Count > 1)
        {
            foreach (Collider2D collision in collisions)
            {
                Vector2 collisionPos = new Vector2(collision.transform.position.x, collision.transform.position.y);
                float collisionDist = Vector2.Distance(mousePos, collisionPos);

                if (collisionDist < selectedCardDist) 
                {
                    selectedCard = collision.GetComponent<PlayCard>();
                    selectedCardDist = collisionDist;
                }
            }
        }

        // Show zoomed card when allowed
        if (!disallowedCardCollisions.Contains(selectedCard.BoxCollider))
        {
            UpdateSelectedCardRpc(selectedCard.NetworkObjectId);
            SetSelectedCardZoomRpc();
            _selectedCard.OnSelected(PickupDisabled);
        }

        OnCardSelected?.Invoke(_player, selectedCard);
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
            bool isUtilityCard = _selectedCard.CardData.Type == ICard.CardType.Utility;
            bool isSlotBlocker = false;
            PlayManager playManager = Locator.Instance.PlayManager;
            bool isValidCard = playManager.CheckValidCard(_selectedCard.CardData);
            bool isMyTurn = _player.IsPlayer1 == playManager.IsPlayer1Turn;
            bool isMyBoard = _player.IsPlayer1 == Locator.Instance.GameBoard.IsSlotOnPlayer1Board(cardSlot);
            bool isUtilitySlot = cardSlot.Type == CardSlot.SlotType.Utility;
            bool isCoreSlot = cardSlot.Type == CardSlot.SlotType.Core;
            bool canPlace = false;
            
            if (isUtilityCard)
            {
                // Handle utility card placement
                var selectedUtilityCard = (UtilityCard)_selectedCard.CardData;
                isSlotBlocker = selectedUtilityCard.UtilityType == UtilityCard.UtilityCardType.SlotBlocker;

                canPlace = (isSlotBlocker && !isUtilitySlot && !isCoreSlot && !isMyBoard) || (!isSlotBlocker && isUtilitySlot);
            }
            else
            {
                // Handle core/peripheral card placement
                if (isMyTurn && !isUtilitySlot && isValidCard) canPlace = true;
            }

            if (canPlace && cardSlot.TryPlaceCard(_selectedCard.gameObject, isSlotBlocker))
            {
                // Card placement allowed, place the card!
                Locator.Instance.CardManager.RemoveCardFromPlayer(_selectedCard.gameObject, _player.IsPlayer1);
                _selectedCard.SetIsBeingDraggedRpc(false);
                OnPlaceCard?.Invoke(_player, _selectedCard.gameObject, cardSlot);
                placed = true;
            }
        }

        if (!placed) // Placing into card slot failed, reset card
        {
            _selectedCard.SetIsBeingDraggedRpc(false);
            _selectedCard.ResetTransformRpc();
        }
    }
    #endregion

    #region Public
    public override async void OnNetworkSpawn()
    {
        while (Locator.Instance.UIManager == null) await Awaitable.NextFrameAsync();
        _selectedCardZoom = Locator.Instance.UIManager.ZoomCard;

        SubToInputEvents();        
    }
    #endregion
}
