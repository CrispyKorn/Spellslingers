using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    PlayCard selectedCard = null;
    ContactFilter2D contactFilter;
    string[] contactLayers = { "CardSlot", "Card" };
    SpriteRenderer selectedCardZoom;

    public NetworkVariable<int> hp = new NetworkVariable<int>(30);
    public Deck hand;
    public event Action<Player, GameObject> OnPlaceCard;
    public event Action<Player, PlayCard> OnCardSelected;

    private void OnEnable()
    {
        if (!IsSpawned || !IsOwner) return;
        SubToInputEvents();
    }

    private void OnDisable()
    {
        if (!IsSpawned || !IsOwner) return;
        UnsubFromInputEvents();
    }

    public override void OnNetworkSpawn()
    {
        hand = new Deck();

        if (!IsOwner) return;

        if (PlayerInput.instance != null) SubToInputEvents();
        else StartCoroutine(WaitForGameManagerInitialization());
    }

    private IEnumerator WaitForGameManagerInitialization()
    {
        while (PlayerInput.instance == null) yield return new WaitForSeconds(1f);
        SubToInputEvents();
        selectedCardZoom = FindObjectOfType<PlayManager>().selectedCard;
    }

    private void SubToInputEvents()
    {
        PlayerInput.instance.inputActions.Battle.Select.started += Select_started;
        PlayerInput.instance.inputActions.Battle.Select.canceled += Select_canceled;
        PlayerInput.instance.inputActions.Battle.Flip.started += Flip_started;
    }

    private void UnsubFromInputEvents()
    {
        PlayerInput.instance.inputActions.Battle.Select.started -= Select_started;
        PlayerInput.instance.inputActions.Battle.Select.canceled -= Select_canceled;
        PlayerInput.instance.inputActions.Battle.Flip.started -= Flip_started;
    }

    private Vector2 GetMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return mousePos;
    }

    private void Select_started(InputAction.CallbackContext obj)
    {
        contactFilter.SetLayerMask(LayerMask.GetMask(contactLayers[1]));
        List<Collider2D> collisions = new List<Collider2D>();
        int numOfHits = Physics2D.OverlapPoint(GetMousePos(), contactFilter, collisions);

        //Only select cards that the player is allowed to see
        List<Collider2D> incorrectCollisions  = new List<Collider2D>();
        foreach (Collider2D collision in collisions)
        {
            if (!collision.GetComponent<PlayCard>().IsOwner && !collision.GetComponent<PlayCard>().faceUp)
            {
                incorrectCollisions.Add(collision);
            }
        }
        /*
        foreach (Collider2D collision in incorrectCollisions)
        {
            numOfHits--;
            collisions.Remove(collision);
        }
        */
        if (numOfHits == 0)
        {
            selectedCard = null;
            selectedCardZoom.gameObject.SetActive(false);
            return;
        }

        PlayCard topCard = collisions[0].GetComponent<PlayCard>();
        if (collisions.Count > 1)
        {
            foreach (Collider2D collision in collisions)
            {
                if (collision.transform.position.z < topCard.transform.position.z) topCard = collision.GetComponent<PlayCard>();
            }
        }

        selectedCard = topCard;

        bool allowedToSeeCard = true;
        if (incorrectCollisions.Contains(selectedCard.boxCollider)) allowedToSeeCard = false;

        if (allowedToSeeCard)
        {
            selectedCardZoom.sprite = selectedCard.cardData.FrontImg;
            selectedCardZoom.gameObject.SetActive(true);
            selectedCard.OnSelected();
        }

        OnCardSelected?.Invoke(this, selectedCard);
    }

    private void Select_canceled(InputAction.CallbackContext obj)
    {
        if (selectedCard == null) return;
        if (!selectedCard.moveable.Value) return;

        bool placed = false;

        contactFilter.SetLayerMask(LayerMask.GetMask(contactLayers[0]));
        List<Collider2D> collisions = new List<Collider2D>();
        int numOfOverlaps = selectedCard.boxCollider.Overlap(contactFilter, collisions);

        if (numOfOverlaps > 0)
        {
            CardSlot cardSlot = collisions[0].GetComponent<CardSlot>();
            float minDist = Vector2.Distance(selectedCard.transform.position, cardSlot.transform.position);
            float dist = 0f;

            for (int i = 1; i < collisions.Count; i++)
            {
                dist = Vector2.Distance(selectedCard.transform.position, collisions[i].transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    cardSlot = collisions[i].GetComponent<CardSlot>();
                }
            }
            
            bool isUtility = selectedCard.cardData.Type == ICard.CardType.Utility;
            bool isValidCard = FindObjectOfType<PlayManager>().CheckValidCard(selectedCard.cardData);
            bool canPlace = cardSlot.IsUtilitySlot ? isUtility : isValidCard;

            if (canPlace)
            {
                if (cardSlot.TryPlaceCard(selectedCard.gameObject))
                {
                    selectedCard.dragging = false;
                    OnPlaceCard?.Invoke(this, selectedCard.gameObject);
                    placed = true;
                }
            }
        }

        if (!placed)
        {
            selectedCard.dragging = false;
            selectedCard.ResetPosServerRpc();
        }
    }

    private void Flip_started(InputAction.CallbackContext obj)
    {
        if (selectedCard == null) return;
        selectedCard.Flip();
    }
}
