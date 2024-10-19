using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayCard : NetworkBehaviour
{
    SpriteRenderer spriteRenderer;
    Vector2 dragOffset;

    public ICard cardData;
    public bool faceUp;
    [HideInInspector] public Vector3 cardPos;
    [HideInInspector] public BoxCollider2D boxCollider;
    [HideInInspector] public bool dragging = false;

    public NetworkVariable<bool> moveable = new NetworkVariable<bool>(true);

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (cardData == null) cardData = FindObjectOfType<CardManager>().CardIndexToCard[0];
    }

    public void Flip()
    {
        faceUp = !faceUp;
        UpdateSprite();
    }

    [ClientRpc]
    public void FlipToClientRpc(bool _faceUp, bool includeInvoker = true)
    {
        if (!includeInvoker && IsHost) return;
        faceUp = _faceUp;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        spriteRenderer.sprite = faceUp ? cardData.FrontImg : cardData.BackImg;
    }

    private Vector2 GetMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return mousePos;
    }

    private IEnumerator DragRoutine()
    {
        while (dragging)
        {
            //Move to mouse pos
            transform.position = GetMousePos() + dragOffset;
            yield return null;
        }

        yield break;
    }

    public void OnSelected()
    {
        cardData.PrintDataToConsole();

        if (!moveable.Value) return;
        dragging = true;
        dragOffset = (Vector2)transform.position - GetMousePos();
        StartCoroutine(DragRoutine());
    }

    [ClientRpc]
    public void SetCardPosClientRpc(Vector3 _cardPos)
    {
        cardPos = _cardPos;
    }

    [ServerRpc]
    public void ResetPosServerRpc()
    {
        ResetPosClientRpc();
    }

    [ClientRpc]
    public void ResetPosClientRpc()
    {
        if (IsOwner) transform.position = cardPos;
    }

    [ClientRpc]
    public void SetCardDataClientRpc(int cardIndex)
    {
        cardData = FindObjectOfType<CardManager>().CardIndexToCard[cardIndex];
        name = cardData.CardName;
        UpdateSprite();
    }
}
