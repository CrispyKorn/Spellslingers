using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayCard : NetworkBehaviour
{
    public bool Moveable { get => _moveable.Value; set => _moveable.Value = value; }
    public ICard CardData { get => _cardData; set => _cardData = value; }
    public bool IsFaceUp { get => _isFaceUp; set => _isFaceUp = value; }
    public BoxCollider2D BoxCollider { get => _boxCollider; }
    public bool IsBeingDragged { get => _isBeingDragged; set => _isBeingDragged = value; }

    private NetworkVariable<bool> _moveable = new(true);

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;
    private ICard _cardData;
    private Vector3 _cardPos;
    private bool _isFaceUp;
    private Vector2 _dragOffset;
    private bool _isBeingDragged = false;

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
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();

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

        if (!_moveable.Value) return;
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

    [ClientRpc]
    public void SetCardPosClientRpc(Vector3 cardPos)
    {
        _cardPos = cardPos;
    }

    [ClientRpc]
    public void ResetPosClientRpc()
    {
        if (IsOwner) transform.position = _cardPos;
    }

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
