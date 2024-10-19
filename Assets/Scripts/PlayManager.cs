using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

public class PlayManager : NetworkBehaviour
{
    GameStateManager gameStateManager;
    CardManager cardManager;
    UtilityManager utilityManager;
    GameBoard board;
    Player player1, player2;
    ulong player2ClientId;
    UIManager uiManager;
    bool extendP1Turn, extendP2Turn;
    
    NetworkVariable<int> currentGameState = new NetworkVariable<int>(2);
    NetworkVariable<bool> gameStateNotUpdated = new NetworkVariable<bool>(true);
    NetworkVariable<bool> p1Attacking = new NetworkVariable<bool>(true);

    public SpriteRenderer selectedCard;

    public Player Player1 { get { return player1; } }
    public Player Player2 { get { return player2; } }

    public override void OnNetworkSpawn()
    {
        uiManager = GetComponent<UIManager>();
        uiManager.BtnPass.onClick.AddListener(OnEndTurnBtnClickedServerRpc);

        if (IsHost)
        {
            cardManager = GetComponent<CardManager>();
            utilityManager = GetComponent<UtilityManager>();
            board = GetComponent<GameBoard>();
            gameStateManager = new GameStateManager(this, cardManager);

            RelayManager relayManager = FindObjectOfType<RelayManager>();
            player2ClientId = relayManager.player2ClientId;
            player1 = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
            player2 = NetworkManager.ConnectedClients[relayManager.player2ClientId].PlayerObject.GetComponent<Player>();

            gameStateManager.OnGameStateChanged += GameStateManager_OnGameStateChanged;
            gameStateManager.OnRoundEnd += GameStateManager_OnRoundEnd;
            gameStateManager.battle.OnDamageDealt += DealDamage;
            gameStateManager.OnStateUpdated += GameStateManager_OnStateUpdated;

            utilityManager.Initialize(new UtilityManager.UtilityInfo(cardManager, Player1, Player2));

            Invoke("SetupGame", 3f);
        }
        else
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            selectedCard.flipX = true;
            selectedCard.flipY = true;
        }
    }

    private void GameStateManager_OnStateUpdated()
    {
        gameStateNotUpdated.Value = false;
    }

    private void SetupGame()
    {
        cardManager.PopulateDecks();

        InitializePlayer(ref player1, ref cardManager.player1CardsIndices, true);
        InitializePlayer(ref player2, ref cardManager.player2CardsIndices, false);

        AssignPlayersClientRpc(player1.NetworkObjectId, player2.NetworkObjectId);

        OnSetupClientRpc();

        //Game Start
        gameStateManager.SetState(gameStateManager.player1Turn, board);
    }

    private void InitializePlayer(ref Player player, ref NetworkList<int> playerCardsIndices, bool IsLocalPlayer)
    {
        player.hand.AddCards(cardManager.Draw(cardManager.CoreDeck, 3));
        player.hand.AddCards(cardManager.Draw(cardManager.OffenceDeck, 6));
        player.hand.AddCards(cardManager.Draw(cardManager.DefenceDeck, 6));
        player.hand.AddCards(cardManager.Draw(cardManager.UtilityDeck, 3));

        List<int> cardIndices = cardManager.GetCardIndices(player.hand.cards);
        foreach (int cardIndex in cardIndices) playerCardsIndices.Add(cardIndex);

        _ = cardManager.InstantiateCards(player.hand.cards, IsLocalPlayer, player2ClientId, player1, player2);
    }

    public bool CheckValidCard(ICard card)
    {
        if (gameStateNotUpdated.Value) return card.Type == ICard.CardType.Core;
        if (currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn) return p1Attacking.Value ? card.Type == ICard.CardType.Offence : card.Type == ICard.CardType.Defence;
        if (currentGameState.Value == (int)GameStateManager.GameStateIndex.Player2Turn) return p1Attacking.Value ? card.Type == ICard.CardType.Defence : card.Type == ICard.CardType.Offence;

        return false;
    }

    private bool CheckSpecialCard(string cardName)
    {
        bool isSpecialCard = false;
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

    [ServerRpc(RequireOwnership = false)]
    private void PlayCardServerRpc(ulong placingPlayerNetworkId, ulong cardObjNetworkId)
    {
        //Extrapolate objects
        Player placingPlayer = GetNetworkObject(placingPlayerNetworkId).GetComponent<Player>();
        GameObject cardObj = GetNetworkObject(cardObjNetworkId).gameObject;
        PlayCard card = cardObj.GetComponent<PlayCard>();
        ICard cardData = card.cardData;

        //Handle card
        if (cardData.Type == ICard.CardType.Core) card.FlipToClientRpc(false);
        else card.FlipToClientRpc(true);

        //Handle hand
        if (cardData.Type != ICard.CardType.Utility) placingPlayer.hand.RemoveCard(cardData);
        bool isPlayer1Turn = placingPlayer == player1;
        cardManager.RemoveFromPlayerHand(isPlayer1Turn ? cardManager.Player1Cards : cardManager.Player2Cards, cardObj);
        _ = cardManager.SpreadCards(isPlayer1Turn ? cardManager.Player1Cards : cardManager.Player2Cards, isPlayer1Turn);

        //Play Card
        if (cardData.Type == ICard.CardType.Utility)
        {
            gameStateManager.SetState(gameStateManager.interrupt, board);

            UtilityCard utilityCard = cardData as UtilityCard;
            utilityCard.OnCardEffectComplete += UtilityCard_OnCardEffectComplete;
            utilityManager.ApplyUtilityEffect(utilityCard, isPlayer1Turn);

            cardManager.UpdatePlayerCards(player2ClientId, player1.transform, player2.transform);
        }
        else PlayTurn(isPlayer1Turn);
    }

    private void UtilityCard_OnCardEffectComplete(UtilityCard utilityCard, Deck playerHand)
    {
        gameStateManager.SetState(gameStateManager.PrevState, board, false);

        playerHand.cards.Remove(utilityCard);
        cardManager.AddToDeck(utilityCard);
        utilityCard.OnCardEffectComplete -= UtilityCard_OnCardEffectComplete;
    }

    private void PlayTurn(bool isPlayer1Turn)
    {
        //Check for end of turn
        CardSlot[] playerBoard = isPlayer1Turn ? board.player1Board : board.player2Board;
        bool p3SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot3].HasCard;
        bool p4SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot4].HasCard;
        bool p5SlotHasCard = playerBoard[(int)GameBoard.Slot.PeripheralSlot5].HasCard;

        if ((p3SlotHasCard && !p4SlotHasCard) || p5SlotHasCard)
        {
            EndTurn();
        }
        else
        {
            gameStateManager.UpdateState();

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

        if (endingAttackingPlayersTurn && (extendP1Turn == extendP2Turn))
        {
            if (extendP1Turn && extendP2Turn) gameStateManager.FlipTurn(board, false);
            else gameStateManager.FlipTurn(board);
        }
        else EndRound();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnEndTurnBtnClickedServerRpc()
    {
        CardSlot[] playerBoard = currentGameState.Value == (int)GameStateManager.GameStateIndex.Player1Turn ? board.player1Board : board.player2Board;

        if (!playerBoard[(int)GameBoard.Slot.PeripheralSlot1].HasCard) gameStateManager.UpdateState();
        if (!playerBoard[(int)GameBoard.Slot.PeripheralSlot2].HasCard) gameStateManager.UpdateState();

        EndTurn();
    }

    private async void EndRound()
    {
        PlayCard p1CoreCard = board.player1Board[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>();
        PlayCard p2CoreCard = board.player2Board[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>();

        if (!p1CoreCard.faceUp)
        {
            p1CoreCard.FlipToClientRpc(true, true);
            p2CoreCard.FlipToClientRpc(true, true);
            
            extendP1Turn = CheckSpecialCard(p1CoreCard.cardData.CardName);
            extendP2Turn = CheckSpecialCard(p2CoreCard.cardData.CardName);

            if (extendP1Turn || extendP2Turn)
            {
                bool extendAttackingPlayersTurn = p1Attacking.Value ? extendP1Turn : extendP2Turn;

                if (extendAttackingPlayersTurn)
                {
                    gameStateManager.FlipTurn(board, false);
                    gameStateManager.UpdateState();
                    SetPassBtnClientRpc(extendP1Turn, !extendP1Turn);
                    return;
                }
                else
                {
                    gameStateManager.UpdateState();
                    SetPassBtnClientRpc(!extendP2Turn, extendP2Turn);
                    return;
                }
            }
        }

        SetPassBtnClientRpc(false, false);

        await Task.Delay(3000);

        //Battle Phase Begin
        gameStateManager.SetState(gameStateManager.battle, board);

        //End Round
        extendP1Turn = false;
        extendP2Turn = false;

        List<ICard> newCards = new List<ICard>();
        newCards.Add(cardManager.Draw(cardManager.OffenceDeck, 1)[0]);
        newCards.Add(cardManager.Draw(cardManager.DefenceDeck, 1)[0]);
        newCards.Add(cardManager.Draw(cardManager.CoreDeck, 1)[0]);
        newCards.Add(cardManager.Draw(cardManager.UtilityDeck, 1)[0]);
        player1.hand.AddCards(newCards);

        await cardManager.InstantiateCards(newCards, true, player2ClientId, player1, player2);

        newCards.Clear();
        newCards.Add(cardManager.Draw(cardManager.OffenceDeck, 1)[0]);
        newCards.Add(cardManager.Draw(cardManager.DefenceDeck, 1)[0]);
        newCards.Add(cardManager.Draw(cardManager.CoreDeck, 1)[0]);
        newCards.Add(cardManager.Draw(cardManager.UtilityDeck, 1)[0]);
        player2.hand.AddCards(newCards);

        await cardManager.InstantiateCards(newCards, false, player2ClientId, player1, player2);

        gameStateManager.ChangeRound();
        gameStateManager.SetState(p1Attacking.Value ? gameStateManager.player1Turn : gameStateManager.player2Turn, board);
    }

    public void DealDamage(bool player1Attacking, int damage)
    {
        Player playerHurt = player1Attacking ? player2 : player1;
        playerHurt.hp.Value -= damage;

        //Update UI
        uiManager.UpdateUI(player1.hp.Value, player2.hp.Value, currentGameState.Value);

        //Check Game Over
        if (playerHurt.hp.Value <= 0)
        {
            gameStateManager.SetState(gameStateManager.interrupt, board);
            GameOverClientRpc(player1Attacking);
        }
    }

    [ClientRpc]
    private void OnSetupClientRpc()
    {
        player1.hp.OnValueChanged += OnPlayerHealthChanged;
        player2.hp.OnValueChanged += OnPlayerHealthChanged;
    }

    [ClientRpc]
    private void AssignPlayersClientRpc(ulong player1Id, ulong player2Id)
    {
        player1 = GetNetworkObject(player1Id).GetComponent<Player>();
        player1.name = "Player 1";
        player2 = GetNetworkObject(player2Id).GetComponent<Player>();
        player2.name = "Player 2";

        if (IsHost) player1.OnPlaceCard += OnPlaceCard;
        else player2.OnPlaceCard += OnPlaceCard;
    }

    [ClientRpc]
    private void UpdateUIClientRpc()
    {
        uiManager.UpdateUI(player1.hp.Value, player2.hp.Value, currentGameState.Value);
    }

    [ClientRpc]
    private void SetPassBtnClientRpc(bool forPlayer1, bool forPlayer2)
    {
        bool activeForMe = IsHost ? forPlayer1 : forPlayer2;

        uiManager.ChangePassBtnVisibility(activeForMe);
    }

    [ClientRpc]
    private void OnGameStateChangedClientRpc(int currentState)
    {
        uiManager.UpdateUI(player1.hp.Value, player2.hp.Value, currentState);
    }

    [ClientRpc]
    private void GameOverClientRpc(bool player1Winner)
    {
        uiManager.GameOverUI(player1Winner);
    }

    private void OnPlayerHealthChanged(int previous, int current)
    {
        if (IsHost) UpdateUIClientRpc();
    }

    private void GameStateManager_OnRoundEnd(bool _p1Attacking)
    {
        p1Attacking.Value = _p1Attacking;
    }

    private void GameStateManager_OnGameStateChanged(int gameState)
    {
        currentGameState.Value = gameState;
        OnGameStateChangedClientRpc(gameState);
    }
}
