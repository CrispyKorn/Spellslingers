using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Utility Card/Sabotage", fileName = "Sabotage")]
public class Sabotage : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private PlayerManager _playerManager;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        // Check for invalid play
        CardSlot[] playerBoard = utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayManager.Board.Player1Board : Locator.Instance.PlayManager.Board.Player2Board;
        if (!Array.Exists(playerBoard, slot => !slot.HasCard))
        {
            OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1, false);
            return;
        }

        // Save data for later
        _utilityInfo = utilityInfo;
        _playerManager = Locator.Instance.PlayerManager;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player1 : _playerManager.Player2;
        _placingPlayer.PickupDisabled = true;
        _placingPlayer.OnCardSelected += OnCardSelected;

    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        // Check for invalid card selection
        ICard.CardType cardType = selectedCard.CardData.Type;
        bool placingPlayerAttacking = Locator.Instance.PlayManager.StateManager.P1Attacking == _utilityInfo.ActivatedByPlayer1;
        ICard.CardType requiredCardType = placingPlayerAttacking ? requiredCardType = ICard.CardType.Defence : requiredCardType = ICard.CardType.Offence;
        bool cardNotFromHand = !_placingPlayer.Hand.CardObjs.Contains(selectedCard.gameObject);
        bool invalidCardType = cardType != requiredCardType && cardType != ICard.CardType.Core;
        CardSlot[] opponentBoard = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayManager.Board.Player2Board : Locator.Instance.PlayManager.Board.Player1Board;
        CardSlot chosenCardSlot = FindValidCardSlot(opponentBoard, cardType);
        
        if (cardNotFromHand || invalidCardType || chosenCardSlot == null)
        {
            _placingPlayer.PickupDisabled = false;
            _placingPlayer.OnCardSelected -= OnCardSelected;
            OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, false);
            return;
        }
        
        // Add the card to the enemy spell
        CardManager cardManager = Locator.Instance.CardManager;
        cardManager.RemoveCardFromPlayer(selectedCard.gameObject, _utilityInfo.ActivatedByPlayer1);
        cardManager.InstantiateCardToSlot(selectedCard.CardData, chosenCardSlot, cardType != ICard.CardType.Core);
        cardManager.DiscardCard(selectedCard);

        //Finish
        Locator.Instance.PlayManager.StateManager.UpdateState();
        _placingPlayer.PickupDisabled = false;
        _placingPlayer.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, true);
    }

    /// <summary>
    /// Attempts to find a valid card slot to place the given card type into.
    /// </summary>
    /// <param name="board">The board to find the valid card slot.</param>
    /// <param name="cardType">The card type of the card being placed.</param>
    /// <returns>The found card slot, or null if no valid slots were found.</returns>
    private CardSlot FindValidCardSlot(CardSlot[] board, ICard.CardType cardType)
    {
        CardSlot currentSlot = board[(int)GameBoard.Slot.CoreSlot];

        // Check core 
        if (cardType == ICard.CardType.Core)
        {
            if (!currentSlot.HasCard) return currentSlot;
            else return null;
        }

        // Check peripheral
        bool isP1ExtendedTurn = Locator.Instance.PlayManager.CurrentGameState == (int)GameStateManager.GameStateIndex.Player1ExtendedTurn;
        bool isP2ExtendedTurn = Locator.Instance.PlayManager.CurrentGameState == (int)GameStateManager.GameStateIndex.Player2ExtendedTurn;
        bool isExtendedTurn = (!_utilityInfo.ActivatedByPlayer1 && isP1ExtendedTurn) || (_utilityInfo.ActivatedByPlayer1  && isP2ExtendedTurn);
        int endNum = isExtendedTurn ? 5 : 3;

        for (int i = 1; i <= endNum; i++)
        {
            currentSlot = board[i];
            if (!currentSlot.HasCard) return currentSlot;
        }
        
        return null;
    }
}
