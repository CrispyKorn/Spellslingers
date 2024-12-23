using Unity.Netcode;
using UnityEngine;

public class PlayCard : NetworkBehaviour
{
    public bool Placed { get => n_placed.Value; set => n_placed.Value = value; }
    public ICard CardData { get => _cardData; }
    public bool IsFaceUp { get => _isFaceUp; }
    public BoxCollider2D BoxCollider { get => _boxCollider; }
    public bool IsBeingDragged { get => _isBeingDragged; set => _isBeingDragged = value; }

    private NetworkVariable<bool> n_placed = new();

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;
    private ICard _cardData;
    private Vector3 _savedCardPos;
    private bool _isFaceUp;
    private bool _isBeingDragged;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Sets the card sprite based on whether it is face-up or face-down
    /// </summary>
    private void UpdateSprite()
    {
        _spriteRenderer.sprite = _isFaceUp ? _cardData.FrontImg : _cardData.BackImg;
    }

    [Rpc(SendTo.Owner)]
    private void FollowMouseRpc()
    {
        FollowMouse();
    }

    private async void FollowMouse()
    {
        while (_isBeingDragged)
        {
            transform.position = Locator.Instance.InputManager.MousePos;
            await Awaitable.NextFrameAsync();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (_cardData == null) _cardData = Locator.Instance.CardManager.CardIndexToCard[0];
    }

    public void Flip()
    {
        _isFaceUp = !_isFaceUp;
        UpdateSprite();
    }

    public void OnSelected()
    {
        _cardData.PrintDataToConsole();

        if (n_placed.Value) return;

        SetIsBeingDraggedRpc(true);
        FollowMouseRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void FlipToRpc(bool faceUp, bool includeInvoker = true)
    {
        if (!includeInvoker && IsHost) return;
        _isFaceUp = faceUp;
        UpdateSprite();
    }

    /// <summary>
    /// Sets the card's positional data, but does not move it there.
    /// </summary>
    /// <param name="cardPos">The position to set in world units.</param>
    [Rpc(SendTo.Everyone)]
    public void SetCardPosRpc(Vector3 cardPos)
    {
        _savedCardPos = cardPos;
    }

    /// <summary>
    /// Moves the card to its saved position.
    /// </summary>
    [Rpc(SendTo.Everyone)]
    public void ResetPosRpc()
    {
        if (IsOwner) transform.position = _savedCardPos;
    }

    /// <summary>
    /// Sets the card data of the playcard.
    /// </summary>
    /// <param name="cardIndex">The index of the card list whose data to use.</param>
    [Rpc(SendTo.Everyone)]
    public void SetCardDataRpc(int cardIndex)
    {
        _cardData = Locator.Instance.CardManager.CardIndexToCard[cardIndex];
        name = _cardData.CardName;
        UpdateSprite();
    }

    [Rpc(SendTo.Everyone)]
    public void SetIsBeingDraggedRpc(bool value)
    {
        _isBeingDragged = value;
    }
}
