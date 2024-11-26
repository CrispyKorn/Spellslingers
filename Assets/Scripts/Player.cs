using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public event Action<Player, GameObject> OnPlaceCard;
    public event Action<Player, PlayCard> OnCardSelected;

    public int Health { get => n_health.Value; set => n_health.Value = value; }
    public NetworkVariable<int> N_Health { get => n_health; }
    public Deck Hand { get => _hand; set => _hand = value; }

    private NetworkVariable<int> n_health = new(30);

    private Deck _hand;
    private PlayCard _selectedCard = null;
    private ContactFilter2D _contactFilter;
    private string[] _contactLayers = { "CardSlot", "Card" };
    private SpriteRenderer _selectedCardZoom;

    private async void OnEnable()
    {
        if (!IsSpawned || !IsOwner) return;

        while (Locator.Instance.InputManager is null) await Awaitable.NextFrameAsync();

        Locator.Instance.InputManager.OnSelect += TrySelectCard;
        Locator.Instance.InputManager.OnSelect_Ended += TryPlaceCard;
        Locator.Instance.InputManager.OnFlip += FlipCard;
    }

    private void OnDisable()
    {
        if (!IsSpawned || !IsOwner) return;

        Locator.Instance.InputManager.OnSelect -= TrySelectCard;
        Locator.Instance.InputManager.OnSelect_Ended -= TryPlaceCard;
        Locator.Instance.InputManager.OnFlip -= FlipCard;
    }

    private Vector2 GetMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return mousePos;
    }

    private void TrySelectCard()
    {
        _contactFilter.SetLayerMask(LayerMask.NameToLayer(_contactLayers[1]));
        List<Collider2D> collisions = new();
        int numOfOverlaps = Physics2D.OverlapPoint(GetMousePos(), _contactFilter, collisions);

        // Only select cards that the player is allowed to see
        List<Collider2D> disallowedCardCollisions  = new();
        foreach (Collider2D collision in collisions)
        {
            var playCard = collision.GetComponent<PlayCard>();
            if (!playCard.IsOwner && !playCard.IsFaceUp) disallowedCardCollisions.Add(collision);
        }

        // End early when no cards are hit
        if (numOfOverlaps == 0) 
        {
            _selectedCard = null;
            _selectedCardZoom.gameObject.SetActive(false);
            return;
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

        _selectedCard = topCard;

        // Show zoomed card when allowed
        var allowedToSeeCard = true;
        if (disallowedCardCollisions.Contains(_selectedCard.BoxCollider)) allowedToSeeCard = false;

        if (allowedToSeeCard)
        {
            _selectedCardZoom.sprite = _selectedCard.CardData.FrontImg;
            _selectedCardZoom.gameObject.SetActive(true);
            _selectedCard.OnSelected();
        }

        OnCardSelected?.Invoke(this, _selectedCard);
    }

    private void TryPlaceCard()
    {
        if (_selectedCard == null || !_selectedCard.Moveable) return;

        var placed = false;

        _contactFilter.SetLayerMask(LayerMask.NameToLayer(_contactLayers[0]));
        List<Collider2D> collisions = new();
        int numOfOverlaps = _selectedCard.BoxCollider.Overlap(_contactFilter, collisions);

        if (numOfOverlaps > 0)
        {
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
            
            bool isUtility = _selectedCard.CardData.Type == ICard.CardType.Utility;
            bool isValidCard = Locator.Instance.PlayManager.CheckValidCard(_selectedCard.CardData);
            bool canPlace = cardSlot.IsUtilitySlot ? isUtility : isValidCard;

            if (canPlace)
            {
                if (cardSlot.TryPlaceCard(_selectedCard.gameObject))
                {
                    _selectedCard.IsBeingDragged = false;
                    OnPlaceCard?.Invoke(this, _selectedCard.gameObject);
                    placed = true;
                }
            }
        }

        if (!placed)
        {
            _selectedCard.IsBeingDragged = false;
            _selectedCard.ResetPosServerRpc();
        }
    }

    private void FlipCard()
    {
        if (_selectedCard == null) return;

        _selectedCard.Flip();
    }

    public override void OnNetworkSpawn()
    {
        Hand = new Deck();

        if (!IsOwner) return;

        _selectedCardZoom = Locator.Instance.PlayManager.SelectedCard;
    }
}
