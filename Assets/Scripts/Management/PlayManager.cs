using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayManager : NetworkBehaviour
{    
    public SpriteRenderer SelectedCard { get => _selectedCard; }
    public int CurrentGameState { get => n_currentGameState.Value; }

    [SerializeField] private SpriteRenderer _selectedCard;

    private NetworkVariable<int> n_currentGameState = new(2);
    private NetworkVariable<bool> n_gameStateNotUpdated = new(true);
    private NetworkVariable<bool> n_p1Attacking = new(true);

    private PlayerManager _playerManager;
    private GameStateManager _gameStateManager;
    private CardManager _cardManager;
    private UtilityManager _utilityManager;
    private UIManager _uiManager;
    private GameBoard _board;
    private bool _extendP1Turn;
    private bool _extendP2Turn;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);

        _playerManager = GetComponent<PlayerManager>();
        _cardManager = GetComponent<CardManager>();
        _utilityManager = GetComponent<UtilityManager>();
        _uiManager = GetComponent<UIManager>();
        _board = GetComponent<GameBoard>();
    }

    private void Start()
    {
        Locator.Instance.InputManager.SetActionMap(InputManager.ActionMap.Player);
    }

    private void SetGameStateUpdated()
    {
        n_gameStateNotUpdated.Value = false;
    }

    private void SetupGame()
    {
        _cardManager.PopulateDecks();
        _cardManager.InitializePlayerCards(_playerManager.Player1, _playerManager.Player2, _playerManager.Player2ClientId);

        TrackCardPlacementClientRpc();

        //Game Start
        _gameStateManager.SetState(_gameStateManager.Player1Turn, _board);
    }

    private bool CheckSpecialCard(string cardName)
    {
        var isSpecialCard = false;

        switch (cardName)
        {
            case "Riptide": isSpecialCard = true; break;
            case "Backdraft": isSpecialCard = true; break;
            case "Static Shock": isSpecialCard = true; break;
        }

        return isSpecialCard;
    }

    private void OnPlaceCard(Player placingPlayer, GameObject cardObj)
    {
        PlayCardServerRpc(placingPlayer.NetworkObjectId, cardObj.GetComponent<NetworkObject>().NetworkObjectId);
    }

    private void UtilityCardCleanup(UtilityCard utilityCard, Deck playerHand)
    {
        _gameStateManager.SetState(_gameStateManager.PrevState, _board, false);

        playerHand.Cards.Remove(utilityCard);
        _cardManager.AddToDeck(utilityCard);
        utilityCard.OnCardEffectComplete -= UtilityCardCleanup;
    }

    private void PlayTurn(bool isPlayer1Turn)
    {
        // Check for end of turn
        CardSlot[] playerBoard = isPlayer1Turn ? _board.player1Board : _board.player2Board;
        bool p3SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot3].HasCard;
        bool p4SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot4].HasCard;
        bool p5SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot5].HasCard;

        if ((p3SlotHasCard && !p4SlotHasCard) || p5SlotHasCard) // All available spaces must be filled to end turn
        {
            EndTurn();
        }
        else
        {
            _gameStateManager.UpdateState();

            SetPassBtnClientRpc(isPlayer1Turn, !isPlayer1Turn);
        }
    }

    private void EndTurn()
    {
        n_gameStateNotUpdated.Value = true;

        bool isPlayer1Turn = n_currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn;
        bool isPlayer2Turn = n_currentGameState.Value == (int)GameStateManager.GameStateIndex.Player2Turn;
        bool endingAttackingPlayersTurn = n_p1Attacking.Value ? isPlayer1Turn : isPlayer2Turn;

        SetPassBtnClientRpc(!isPlayer1Turn, isPlayer1Turn);

        if (endingAttackingPlayersTurn && (_extendP1Turn == _extendP2Turn))
        {
            if (_extendP1Turn && _extendP2Turn) _gameStateManager.FlipTurn(_board, false);
            else _gameStateManager.FlipTurn(_board);
        }
        else EndRound();
    }

    private async void EndRound()
    {
        var p1CoreCard = _board.player1Board[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>();
        var p2CoreCard = _board.player2Board[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>();

        if (!p1CoreCard.IsFaceUp)
        {
            p1CoreCard.FlipToClientRpc(true, true);
            p2CoreCard.FlipToClientRpc(true, true);

            _extendP1Turn = CheckSpecialCard(p1CoreCard.CardData.CardName);
            _extendP2Turn = CheckSpecialCard(p2CoreCard.CardData.CardName);

            if (_extendP1Turn || _extendP2Turn)
            {
                bool extendAttackingPlayersTurn = n_p1Attacking.Value ? _extendP1Turn : _extendP2Turn;

                if (extendAttackingPlayersTurn)
                {
                    _gameStateManager.FlipTurn(_board, false);
                    _gameStateManager.UpdateState();
                    SetPassBtnClientRpc(_extendP1Turn, !_extendP1Turn);
                    return;
                }
                else
                {
                    _gameStateManager.UpdateState();
                    SetPassBtnClientRpc(!_extendP2Turn, _extendP2Turn);
                    return;
                }
            }
        }

        SetPassBtnClientRpc(false, false);

        await Awaitable.WaitForSecondsAsync(3);

        // Battle Phase Begin
        _gameStateManager.SetState(_gameStateManager.Battle, _board);

        // End Round
        _extendP1Turn = false;
        _extendP2Turn = false;

        await _cardManager.AddEndOfRoundCards(_playerManager.Player1, _playerManager.Player2, _playerManager.Player2ClientId);

        _gameStateManager.ChangeRound();
        _gameStateManager.SetState(n_p1Attacking.Value ? _gameStateManager.Player1Turn : _gameStateManager.Player2Turn, _board);
    }

    private void OnPlayerHealthChanged(int previous, int current)
    {
        if (IsHost) UpdateUIClientRpc();
    }

    private void UpdateAttackingPlayer(bool _p1Attacking)
    {
        n_p1Attacking.Value = _p1Attacking;
    }

    private void UpdateGameState(int gameState)
    {
        n_currentGameState.Value = gameState;
        OnGameStateChangedClientRpc(gameState);
    }

    [ClientRpc]
    public void TrackCardPlacementClientRpc()
    {
        if (IsHost) _playerManager.Player1.OnPlaceCard += OnPlaceCard;
        else _playerManager.Player2.OnPlaceCard += OnPlaceCard;
    }

    [ClientRpc]
    public void UpdateUIClientRpc()
    {
        _uiManager.UpdateUI(_playerManager.Player1.Health, _playerManager.Player2.Health, n_currentGameState.Value);
    }

    [ClientRpc]
    private void SetPassBtnClientRpc(bool forPlayer1, bool forPlayer2)
    {
        bool activeForMe = IsHost ? forPlayer1 : forPlayer2;

        _uiManager.ChangePassBtnVisibility(activeForMe);
    }

    [ClientRpc]
    private void OnGameStateChangedClientRpc(int currentState)
    {
        _uiManager.UpdateUI(_playerManager.Player1.Health, _playerManager.Player2.Health, currentState);
    }

    [ClientRpc]
    private void GameOverClientRpc(bool player1Winner)
    {
        _uiManager.GameOverUI(player1Winner);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayCardServerRpc(ulong placingPlayerNetworkId, ulong cardObjNetworkId)
    {
        //Extrapolate objects
        var placingPlayer = GetNetworkObject(placingPlayerNetworkId).GetComponent<Player>();
        GameObject cardObj = GetNetworkObject(cardObjNetworkId).gameObject;
        var card = cardObj.GetComponent<PlayCard>();
        ICard cardData = card.CardData;

        // Handle card
        if (cardData.Type == ICard.CardType.Core) card.FlipToClientRpc(false);
        else card.FlipToClientRpc(true);

        // Handle hand
        if (cardData.Type != ICard.CardType.Utility) placingPlayer.Hand.RemoveCard(cardData);
        bool isPlayer1Turn = placingPlayer == _playerManager.Player1;
        _cardManager.RemoveFromPlayerHand(isPlayer1Turn ? _cardManager.Player1Cards : _cardManager.Player2Cards, cardObj);
        _ = _cardManager.SpreadCards(isPlayer1Turn ? _cardManager.Player1Cards : _cardManager.Player2Cards, isPlayer1Turn);

        // Play Card
        if (cardData.Type == ICard.CardType.Utility)
        {
            _gameStateManager.SetState(_gameStateManager.Interrupt, _board);

            var utilityCard = cardData as UtilityCard;
            utilityCard.OnCardEffectComplete += UtilityCardCleanup;
            _utilityManager.ApplyUtilityEffect(utilityCard, isPlayer1Turn);

            _cardManager.UpdatePlayerCards(_playerManager.Player2ClientId, _playerManager.Player1.transform, _playerManager.Player2.transform);
        }
        else PlayTurn(isPlayer1Turn);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnEndTurnBtnClickedServerRpc()
    {
        CardSlot[] playerBoard = n_currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn ? _board.player1Board : _board.player2Board;

        if (!playerBoard[(int)GameBoard.Slot.PeripheralSlot1].HasCard) _gameStateManager.UpdateState();
        if (!playerBoard[(int)GameBoard.Slot.PeripheralSlot2].HasCard) _gameStateManager.UpdateState();

        EndTurn();
    }

    public override void OnNetworkSpawn()
    {
        _uiManager.BtnPass.onClick.AddListener(OnEndTurnBtnClickedServerRpc);

        if (IsHost)
        {
            _gameStateManager = new GameStateManager(this, _cardManager);

            _gameStateManager.OnGameStateChanged += UpdateGameState;
            _gameStateManager.OnRoundEnd += UpdateAttackingPlayer;
            _gameStateManager.Battle.OnDamageDealt += (b, i) => { _playerManager.DealDamage(b, i); };
            _gameStateManager.OnStateUpdated += SetGameStateUpdated;

            _utilityManager.Initialize(new UtilityInfo(_cardManager, _playerManager.Player1, _playerManager.Player2));

            Invoke("SetupGame", 3f);
        }
        else
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            _selectedCard.flipX = true;
            _selectedCard.flipY = true;
        }
    }

    public bool CheckValidCard(ICard card)
    {
        if (n_gameStateNotUpdated.Value) return card.Type == ICard.CardType.Core;
        if (n_currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn) return n_p1Attacking.Value ? card.Type == ICard.CardType.Offence : card.Type == ICard.CardType.Defence;
        if (n_currentGameState.Value == (int)GameStateManager.GameStateIndex.Player2Turn) return n_p1Attacking.Value ? card.Type == ICard.CardType.Defence : card.Type == ICard.CardType.Offence;

        return false;
    }

    public void GameOver(bool player1Winner)
    {
        _gameStateManager.SetState(_gameStateManager.Interrupt, _board);
        GameOverClientRpc(player1Winner);
    }
}
