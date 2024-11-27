using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayManager : NetworkBehaviour
{
    public Player Player1 { get => _player1; }
    public Player Player2 { get => _player2; }
    public SpriteRenderer SelectedCard { get => _selectedCard; }

    [SerializeField] private SpriteRenderer _selectedCard;

    private NetworkVariable<int> currentGameState = new(2);
    private NetworkVariable<bool> gameStateNotUpdated = new(true);
    private NetworkVariable<bool> p1Attacking = new(true);

    private GameStateManager _gameStateManager;
    private CardManager _cardManager;
    private UtilityManager _utilityManager;
    private GameBoard _board;
    private Player _player1;
    private Player _player2;
    private ulong _player2ClientId;
    private UIManager _uiManager;
    private bool _extendP1Turn;
    private bool _extendP2Turn;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
    }

    private void SetGameStateUpdated()
    {
        gameStateNotUpdated.Value = false;
    }

    private void SetupGame()
    {
        _cardManager.PopulateDecks();

        InitializePlayer(_player1, _cardManager.Player1CardsIndices, true);
        InitializePlayer(_player2, _cardManager.Player2CardsIndices, false);

        AssignPlayersClientRpc(_player1.NetworkObjectId, _player2.NetworkObjectId);

        TrackPlayerHealthClientRpc();

        //Game Start
        _gameStateManager.SetState(_gameStateManager.Player1Turn, _board);
    }

    private void InitializePlayer(Player player, NetworkList<int> playerCardsIndices, bool IsLocalPlayer)
    {
        player.Hand.AddCards(_cardManager.Draw(_cardManager.CoreDeck, 3));
        player.Hand.AddCards(_cardManager.Draw(_cardManager.OffenceDeck, 6));
        player.Hand.AddCards(_cardManager.Draw(_cardManager.DefenceDeck, 6));
        player.Hand.AddCards(_cardManager.Draw(_cardManager.UtilityDeck, 3));

        List<int> cardIndices = _cardManager.GetCardIndices(player.Hand.Cards);
        foreach (int cardIndex in cardIndices) playerCardsIndices.Add(cardIndex);

        _ = _cardManager.InstantiateCards(player.Hand.Cards, IsLocalPlayer, _player2ClientId, _player1, _player2);
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

    private void UtilityCard_OnCardEffectComplete(UtilityCard utilityCard, Deck playerHand)
    {
        _gameStateManager.SetState(_gameStateManager.PrevState, _board, false);

        playerHand.Cards.Remove(utilityCard);
        _cardManager.AddToDeck(utilityCard);
        utilityCard.OnCardEffectComplete -= UtilityCard_OnCardEffectComplete;
    }

    private void PlayTurn(bool isPlayer1Turn)
    {
        // Check for end of turn
        CardSlot[] playerBoard = isPlayer1Turn ? _board.player1Board : _board.player2Board;
        bool p3SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot3].HasCard;
        bool p4SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot4].HasCard;
        bool p5SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot5].HasCard;

        if ((p3SlotHasCard && !p4SlotHasCard) || p5SlotHasCard)
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
        gameStateNotUpdated.Value = true;

        bool isPlayer1Turn = currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn;
        bool isPlayer2Turn = currentGameState.Value == (int)GameStateManager.GameStateIndex.Player2Turn;
        bool endingAttackingPlayersTurn = p1Attacking.Value ? isPlayer1Turn : isPlayer2Turn;

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
                bool extendAttackingPlayersTurn = p1Attacking.Value ? _extendP1Turn : _extendP2Turn;

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

        var newCards = new List<ICard>();
        newCards.Add(_cardManager.DrawOne(_cardManager.OffenceDeck));
        newCards.Add(_cardManager.DrawOne(_cardManager.DefenceDeck));
        newCards.Add(_cardManager.DrawOne(_cardManager.CoreDeck));
        newCards.Add(_cardManager.DrawOne(_cardManager.UtilityDeck));
        _player1.Hand.AddCards(newCards);

        await _cardManager.InstantiateCards(newCards, true, _player2ClientId, _player1, _player2);

        newCards.Clear();
        newCards.Add(_cardManager.DrawOne(_cardManager.OffenceDeck));
        newCards.Add(_cardManager.DrawOne(_cardManager.DefenceDeck));
        newCards.Add(_cardManager.DrawOne(_cardManager.CoreDeck));
        newCards.Add(_cardManager.DrawOne(_cardManager.UtilityDeck));
        _player2.Hand.AddCards(newCards);

        await _cardManager.InstantiateCards(newCards, false, _player2ClientId, _player1, _player2);

        _gameStateManager.ChangeRound();
        _gameStateManager.SetState(p1Attacking.Value ? _gameStateManager.Player1Turn : _gameStateManager.Player2Turn, _board);
    }

    private void OnPlayerHealthChanged(int previous, int current)
    {
        if (IsHost) UpdateUIClientRpc();
    }

    private void UpdateAttackingPlayer(bool _p1Attacking)
    {
        p1Attacking.Value = _p1Attacking;
    }

    private void UpdateGameState(int gameState)
    {
        currentGameState.Value = gameState;
        OnGameStateChangedClientRpc(gameState);
    }

    [ClientRpc]
    private void TrackPlayerHealthClientRpc()
    {
        _player1.N_Health.OnValueChanged += OnPlayerHealthChanged;
        _player2.N_Health.OnValueChanged += OnPlayerHealthChanged;
    }

    [ClientRpc]
    private void AssignPlayersClientRpc(ulong player1Id, ulong player2Id)
    {
        _player1 = GetNetworkObject(player1Id).GetComponent<Player>();
        _player1.name = "Player 1";
        _player2 = GetNetworkObject(player2Id).GetComponent<Player>();
        _player2.name = "Player 2";

        if (IsHost) _player1.OnPlaceCard += OnPlaceCard;
        else _player2.OnPlaceCard += OnPlaceCard;
    }

    [ClientRpc]
    private void UpdateUIClientRpc()
    {
        _uiManager.UpdateUI(_player1.Health, _player2.Health, currentGameState.Value);
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
        _uiManager.UpdateUI(_player1.Health, _player2.Health, currentState);
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
        bool isPlayer1Turn = placingPlayer == _player1;
        _cardManager.RemoveFromPlayerHand(isPlayer1Turn ? _cardManager.Player1Cards : _cardManager.Player2Cards, cardObj);
        _ = _cardManager.SpreadCards(isPlayer1Turn ? _cardManager.Player1Cards : _cardManager.Player2Cards, isPlayer1Turn);

        // Play Card
        if (cardData.Type == ICard.CardType.Utility)
        {
            _gameStateManager.SetState(_gameStateManager.Interrupt, _board);

            var utilityCard = cardData as UtilityCard;
            utilityCard.OnCardEffectComplete += UtilityCard_OnCardEffectComplete;
            _utilityManager.ApplyUtilityEffect(utilityCard, isPlayer1Turn);

            _cardManager.UpdatePlayerCards(_player2ClientId, _player1.transform, _player2.transform);
        }
        else PlayTurn(isPlayer1Turn);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnEndTurnBtnClickedServerRpc()
    {
        CardSlot[] playerBoard = currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn ? _board.player1Board : _board.player2Board;

        if (!playerBoard[(int)GameBoard.Slot.PeripheralSlot1].HasCard) _gameStateManager.UpdateState();
        if (!playerBoard[(int)GameBoard.Slot.PeripheralSlot2].HasCard) _gameStateManager.UpdateState();

        EndTurn();
    }

    public override void OnNetworkSpawn()
    {
        _uiManager = GetComponent<UIManager>();
        _uiManager.BtnPass.onClick.AddListener(OnEndTurnBtnClickedServerRpc);

        if (IsHost)
        {
            _cardManager = GetComponent<CardManager>();
            _utilityManager = GetComponent<UtilityManager>();
            _board = GetComponent<GameBoard>();
            _gameStateManager = new GameStateManager(this, _cardManager);

            RelayManager relayManager = Locator.Instance.RelayManager;
            _player2ClientId = relayManager.Player2ClientId;
            _player1 = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
            _player2 = NetworkManager.ConnectedClients[relayManager.Player2ClientId].PlayerObject.GetComponent<Player>();

            _gameStateManager.OnGameStateChanged += UpdateGameState;
            _gameStateManager.OnRoundEnd += UpdateAttackingPlayer;
            _gameStateManager.Battle.OnDamageDealt += DealDamage;
            _gameStateManager.OnStateUpdated += SetGameStateUpdated;

            _utilityManager.Initialize(new UtilityInfo(_cardManager, Player1, Player2));

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
        if (gameStateNotUpdated.Value) return card.Type == ICard.CardType.Core;
        if (currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn) return p1Attacking.Value ? card.Type == ICard.CardType.Offence : card.Type == ICard.CardType.Defence;
        if (currentGameState.Value == (int)GameStateManager.GameStateIndex.Player2Turn) return p1Attacking.Value ? card.Type == ICard.CardType.Defence : card.Type == ICard.CardType.Offence;

        return false;
    }

    public void DealDamage(bool player1Attacking, int damage)
    {
        Player playerHurt = player1Attacking ? _player2 : _player1;
        playerHurt.Health -= damage;

        // Update UI
        _uiManager.UpdateUI(_player1.Health, _player2.Health, currentGameState.Value);

        // Check Game Over
        if (playerHurt.Health <= 0)
        {
            _gameStateManager.SetState(_gameStateManager.Interrupt, _board);
            GameOverClientRpc(player1Attacking);
        }
    }
}
