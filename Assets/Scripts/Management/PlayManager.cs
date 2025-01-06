using UnityEngine;
using Unity.Netcode;

public class PlayManager : NetworkBehaviour
{    
    public SpriteRenderer SelectedCard { get => _selectedCard; }
    public int CurrentGameState { get => _gameStateManager.CurrentStateIndex; }
    public bool IsPlayer1Turn { get => _gameStateManager.CurrentStateIndex == (int)GameStateManager.GameStateIndex.Player1Turn 
                                    || _gameStateManager.CurrentStateIndex == (int)GameStateManager.GameStateIndex.Player1ExtendedTurn; }
    public GameBoard Board { get => _board; }

    [SerializeField] private SpriteRenderer _selectedCard;

    private PlayerManager _playerManager;
    private GameStateManager _gameStateManager;
    private CardManager _cardManager;
    private UtilityManager _utilityManager;
    private UIManager _uiManager;
    private GameBoard _board;

    private int count;

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
        Locator.Instance.InputManager.SetActionMap(InputManager.ActionMap.Battle);
    }

    #region Private
    /// <summary>
    /// Sets up the game on a high level, initializing players, and cards before starting the game.
    /// </summary>
    private void SetupGame()
    {
        _cardManager.PopulateDecks();
        _cardManager.InitializePlayerCards(_playerManager.Player1, _playerManager.Player2, _playerManager.Player2ClientId);

        _playerManager.Player1.OnPlaceCard += PlayCard;
        _playerManager.Player2.OnPlaceCard += PlayCard;

        //Game Start
        _gameStateManager.SetState(_gameStateManager.Player1Turn, _board);
    }

    /// <summary>
    /// Run when a card is played by a player.
    /// </summary>
    /// <param name="placingPlayer">The player placing the card.</param>
    /// <param name="cardObj">The placed card.</param>
    /// <param name="cardSlot">The card slot the card was placed into.</param>
    private void PlayCard(Player placingPlayer, GameObject cardObj, CardSlot cardSlot)
    {
        NetworkBehaviourReference cardSlotNetworkReference = new(cardSlot);
        PlayCardRpc(placingPlayer.NetworkObjectId, cardObj.GetComponent<NetworkObject>().NetworkObjectId, cardSlotNetworkReference);
    }

    /// <summary>
    /// Run when a utility card effect is complete. Cleans up the game state and cards.
    /// </summary>
    /// <param name="utilityCard">The utility card that was used.</param>
    /// <param name="playerHand">The player who played the utility card.</param>
    private void UtilityCardCleanup(UtilityCard utilityCard, Deck playerHand)
    {
        _gameStateManager.UpdateState();

        playerHand.Cards.Remove(utilityCard);
        _cardManager.AddToDeck(utilityCard);
        utilityCard.OnCardEffectComplete -= UtilityCardCleanup;
    }

    /// <summary>
    /// Ends a round, moving to the next round
    /// </summary>
    public async void HandleEndOfRound()
    {
        await _cardManager.AddEndOfRoundCards(_playerManager.Player1, _playerManager.Player2, _playerManager.Player2ClientId);
    }

    /// <summary>
    /// Run when the game state is updated. Updates the local value of n_currentGameState and the UI.
    /// </summary>
    /// <param name="gameState"></param>
    private void UpdateGameState(int gameState)
    {
        UpdateUIRpc(_playerManager.Player1.Health, _playerManager.Player2.Health, gameState);
    }

    /// <summary>
    /// Locally sets the UI to the 'Game Over' screen for everyone on the network.
    /// </summary>
    /// <param name="player1Winner"></param>
    [Rpc(SendTo.Everyone)]
    private void GameOverRpc(bool player1Winner)
    {
        _uiManager.GameOverUI(player1Winner);
    }

    /// <summary>
    /// Handles the playing of a card. Run on the server.
    /// </summary>
    /// <param name="placingPlayerNetworkId">The network ID of the player placing the card.</param>
    /// <param name="cardObjNetworkId">The network ID of the placed card.</param>
    [Rpc(SendTo.Server)]
    private void PlayCardRpc(ulong placingPlayerNetworkId, ulong cardObjNetworkId, NetworkBehaviourReference cardSlotNetworkReference)
    {
        //Extrapolate objects
        var placingPlayer = GetNetworkObject(placingPlayerNetworkId).GetComponent<Player>();
        GameObject cardObj = GetNetworkObject(cardObjNetworkId).gameObject;
        var card = cardObj.GetComponent<PlayCard>();
        ICard cardData = card.CardData;
        cardSlotNetworkReference.TryGet(out var cardSlotNetworkObject, NetworkManager);
        var cardSlot = cardSlotNetworkObject.GetComponent<CardSlot>();
        bool isPlayer1Turn = placingPlayer == _playerManager.Player1;

        _cardManager.HandleCardPlaced(cardObj, card, cardData, placingPlayer, isPlayer1Turn);

        // Play Card
        if (cardSlot.Type == CardSlot.SlotType.Utility)
        {
            _gameStateManager.SetState(_gameStateManager.Interrupt, _board);

            var utilityCard = (UtilityCard)cardData;
            utilityCard.OnCardEffectComplete += UtilityCardCleanup;
            _utilityManager.ApplyUtilityEffect(utilityCard, isPlayer1Turn);
        }
        else _gameStateManager.UpdateState();
    }

    /// <summary>
    /// Handles the ending of a turn prematurely. Run on the server.
    /// </summary>
    [Rpc(SendTo.Server)]
    private void EndTurnEarlyRpc()
    {
        _gameStateManager.FinishState();
    }
    #endregion

    #region Public
    public override async void OnNetworkSpawn()
    {
        _uiManager.BtnPass.onClick.AddListener(EndTurnEarlyRpc);

        if (IsHost)
        {
            _gameStateManager = new GameStateManager(this, _cardManager, _board);

            _gameStateManager.OnGameStateChanged += UpdateGameState;
            _gameStateManager.Battle.OnDamageDealt += (b, i) => { _playerManager.DealDamage(b, i); };

            _utilityManager.Initialize(new UtilityInfo(_cardManager, _playerManager.Player1, _playerManager.Player2));

            await Awaitable.WaitForSecondsAsync(3f);
            SetupGame();
        }
        else
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            _selectedCard.flipX = true;
            _selectedCard.flipY = true;

            _uiManager.SwapPlayerHealth();
        }
    }

    /// <summary>
    /// Checks whether the given card is valid to play given the current game state context.
    /// </summary>
    /// <param name="card">The card to validate.</param>
    /// <returns>Whether the card is valid.</returns>
    public bool CheckValidCard(ICard card)
    {
        CardSlot currentPlayerCoreSlot = IsPlayer1Turn ? _board.Player1Board[(int)GameBoard.Slot.CoreSlot] : _board.Player2Board[(int)GameBoard.Slot.CoreSlot];

        // Enforce core cards when slot is vacant
        if (currentPlayerCoreSlot.IsUsable) return card.Type == ICard.CardType.Core;

        // Enforce peripheral cards
        bool isOffenceCard = card.Type == ICard.CardType.Offence;
        bool isDefenceCard = card.Type == ICard.CardType.Defence;

        return (IsPlayer1Turn == _gameStateManager.P1Attacking) ? isOffenceCard : isDefenceCard;
    }

    /// <summary>
    /// Handles 'Game Over' 
    /// </summary>
    /// <param name="player1Winner">Whether player 1 (host) was the winner.</param>
    public void GameOver(bool player1Winner)
    {
        _gameStateManager.SetState(_gameStateManager.Interrupt, _board);
        GameOverRpc(player1Winner);
    }

    /// <summary>
    /// Locally updates the UI for everyone on the network.
    /// </summary>
    /// <param name="player1Health">The current health of player 1 (host).</param>
    /// <param name="player2Health">The current health of player 2 (client).</param>
    /// <param name="currentGameState">The current value of the game state.</param>
    [Rpc(SendTo.Everyone)]
    public void UpdateUIRpc(int player1Health, int player2Health, int currentGameState)
    {
        _uiManager.UpdateUI(player1Health, player2Health, currentGameState, IsHost);
    }
    #endregion
}
