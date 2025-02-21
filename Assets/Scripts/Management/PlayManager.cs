using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class PlayManager : NetworkBehaviour
{    
    public bool IsPlayer1Turn { get => _gameStateManager.CurrentStateIndex == (int)GameStateManager.GameStateIndex.Player1Turn 
                                    || _gameStateManager.CurrentStateIndex == (int)GameStateManager.GameStateIndex.Player1ExtendedTurn; }

    private PlayerManager _playerManager;
    private GameStateManager _gameStateManager;
    private CardManager _cardManager;
    private UtilityManager _utilityManager;
    private UIManager _uiManager;
    private GameBoard _gameBoard;
    private DamageIndicatorManager _damageIndicatorManager;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
    }

    #region Private
    /// <summary>
    /// Sets up the game on a high level, initializing players, and cards before starting the game.
    /// </summary>
    private async void SetupGame()
    {
        _cardManager.PopulateDecks();
        await _cardManager.InitializePlayerCards();

        _playerManager.Player1.Interaction.OnPlaceCard += PlayCard;
        _playerManager.Player2.Interaction.OnPlaceCard += PlayCard;

        //Game Start
        _gameStateManager.SetState(_gameStateManager.Player1Turn);
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
    /// Ends a round, moving to the next round
    /// </summary>
    public async Task HandleEndOfRound()
    {
        await _cardManager.AddEndOfRoundCards();
    }

    /// <summary>
    /// Run when the game state is updated. Updates the UI.
    /// </summary>
    /// <param name="gameState"></param>
    private void UpdateGameState(int gameState)
    {
        // Determine whether to enable/disable pass button
        bool isPlayer1Turn = gameState == (int)GameStateManager.GameStateIndex.Player1Turn || gameState == (int)GameStateManager.GameStateIndex.Player1ExtendedTurn;
        bool isPlayer2Turn = gameState == (int)GameStateManager.GameStateIndex.Player2Turn || gameState == (int)GameStateManager.GameStateIndex.Player2ExtendedTurn;
        bool player1CoreSlotFilled = _gameBoard.Player1Board[(int)GameBoard.Slot.CoreSlot].HasCard;
        bool player2CoreSlotFilled = _gameBoard.Player2Board[(int)GameBoard.Slot.CoreSlot].HasCard;
        bool passBtnActiveForP1 = isPlayer1Turn && player1CoreSlotFilled;
        bool passBtnActiveForP2 = isPlayer2Turn && player2CoreSlotFilled;

        _uiManager.UpdateUIStateRpc(gameState);
        _uiManager.UpdateUIPassBtnRpc(passBtnActiveForP1, passBtnActiveForP2);
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
        var playCard = cardObj.GetComponent<PlayCard>();
        ICard cardData = playCard.CardData;
        cardSlotNetworkReference.TryGet(out var cardSlotNetworkObject, NetworkManager);
        var cardSlot = cardSlotNetworkObject.GetComponent<CardSlot>();
        bool placedByPlayer1 = placingPlayer == _playerManager.Player1;

        // Handle core and peripheral cards
        if (cardData.Type == ICard.CardType.Core) playCard.FlipToRpc(false, false);
        else playCard.FlipToRpc(true, true);

        // Play Card
        if (cardSlot.Type == CardSlot.SlotType.Utility)
        {
            _gameStateManager.SetState(_gameStateManager.Interrupt);

            var utilityCard = (UtilityCard)cardData;
            _utilityManager.ApplyUtilityEffect(new UtilityInfo(placedByPlayer1, utilityCard));
        }
        else 
        {
            if (cardData.Type != ICard.CardType.Core)
            {
                Card card = (Card)cardData;
                _damageIndicatorManager.SetIndicatorsRpc(placedByPlayer1, card.Type, card.Element, card.Values);
            }
            
            _gameStateManager.UpdateState();
        }
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
        _playerManager = Locator.Instance.PlayerManager;
        _cardManager = Locator.Instance.CardManager;
        _utilityManager = Locator.Instance.UtilityManager;
        _uiManager = Locator.Instance.UIManager;
        _gameBoard = Locator.Instance.GameBoard;
        _damageIndicatorManager = Locator.Instance.DamageIndicatorManager;

        Locator.Instance.InputManager.SetActionMap(InputManager.ActionMap.Battle);
        _uiManager.BtnPass.onClick.AddListener(EndTurnEarlyRpc);

        if (IsHost)
        {
            _gameStateManager = new GameStateManager(this, _cardManager, _gameBoard);

            _gameStateManager.OnGameStateChanged += UpdateGameState;
            _gameStateManager.OnStateUpdated += UpdateGameState;
            _gameStateManager.Battle.OnDamageDealt += (b, i) => { _playerManager.DealDamage(b, i); };

            await Awaitable.WaitForSecondsAsync(3f);
            SetupGame();
        }
        else
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            _uiManager.SwapUIElements();
        }
    }

    /// <summary>
    /// Checks whether the given card is valid to play given the current game state context.
    /// </summary>
    /// <param name="card">The card to validate.</param>
    /// <returns>Whether the card is valid.</returns>
    public bool CheckValidCard(ICard card)
    {
        CardSlot currentPlayerCoreSlot = IsPlayer1Turn ? _gameBoard.Player1Board[(int)GameBoard.Slot.CoreSlot] : _gameBoard.Player2Board[(int)GameBoard.Slot.CoreSlot];

        // Enforce core cards when slot is vacant
        if (currentPlayerCoreSlot.IsUsable) return card.Type == ICard.CardType.Core;

        // Enforce peripheral cards
        return card.Type != ICard.CardType.Utility;
    }

    /// <summary>
    /// Handles 'Game Over' 
    /// </summary>
    /// <param name="player1Winner">Whether player 1 (host) was the winner.</param>
    public void GameOver(bool player1Winner)
    {
        _gameStateManager.SetState(_gameStateManager.Interrupt);
        GameOverRpc(player1Winner);
    }
    #endregion
}
