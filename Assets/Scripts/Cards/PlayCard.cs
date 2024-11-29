using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayCard : NetworkBehaviour
{
    public bool Moveable { get => n_moveable.Value; set => n_moveable.Value = value; }
    public ICard CardData { get => _cardData; }
    public bool IsFaceUp { get => _isFaceUp; }
    public BoxCollider2D BoxCollider { get => _boxCollider; }
    public bool IsBeingDragged { get => _isBeingDragged; set => _isBeingDragged = value; }

    private NetworkVariable<bool> n_moveable = new(true);

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;
    private ICard _cardData;
    private Vector3 _savedCardPos;
    private bool _isFaceUp;
    private Vector2 _dragOffset;
    private bool _isBeingDragged = false;

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

    private Vector2 GetMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return mousePos;
    }

    private async void FollowMouse()
    {
        while (_isBeingDragged)
        {
            transform.position = GetMousePos() + _dragOffset;
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

        if (!n_moveable.Value) return;
        _isBeingDragged = true;
        _dragOffset = (Vector2)transform.position - GetMousePos();
        FollowMouse();
    }

    [ClientRpc]
    public void FlipToClientRpc(bool faceUp, bool includeInvoker = true)
    {
        if (!includeInvoker && IsHost) return;
        _isFaceUp = faceUp;
        UpdateSprite();
    }

    /// <summary>
    /// Sets the card's positional data, but does not move it there.
    /// </summary>
    /// <param name="cardPos">The position to set in world units.</param>
    [ClientRpc]
    public void SetCardPosClientRpc(Vector3 cardPos)
    {
        _savedCardPos = cardPos;
    }

    /// <summary>
    /// Moves the card to its saved position.
    /// </summary>
    [ClientRpc]
    public void ResetPosClientRpc()
    {
        if (IsOwner) transform.position = _savedCardPos;
    }

    /// <summary>
    /// Sets the card data of the playcard.
    /// </summary>
    /// <param name="cardIndex">The index of the card list whose data to use.</param>
    [ClientRpc]
    public void SetCardDataClientRpc(int cardIndex)
    {
        _cardData = Locator.Instance.CardManager.CardIndexToCard[cardIndex];
        name = _cardData.CardName;
        UpdateSprite();
    }

    [ServerRpc]
    public void ResetPosServerRpc()
    {
        ResetPosClientRpc();
    }
}
