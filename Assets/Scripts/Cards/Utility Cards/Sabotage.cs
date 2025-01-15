using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Utility Card/Sabotage", fileName = "Sabotage")]
public class Sabotage : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private PlayerManager _playerManager;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        // Check for invalid play
        CardSlot[] playerBoard = utilityInfo.ActivatedByPlayer1 ? Locator.Instance.GameBoard.Player1Board : Locator.Instance.GameBoard.Player2Board;
        if (!Array.Exists(playerBoard, slot => !slot.HasCard))
        {
            OnCardEffectComplete?.Invoke(utilityInfo);
            return;
        }

        // Save data for later
        _utilityInfo = utilityInfo;
        _playerManager = Locator.Instance.PlayerManager;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player1 : _playerManager.Player2;
        _placingPlayer.Interaction.PickupDisabled = true;
        _placingPlayer.Interaction.OnCardSelected += OnCardSelected;

    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        // Check for invalid card selection
        ICard.CardType cardType = selectedCard.CardData.Type;
        bool placingPlayerAttacking = Locator.Instance.GameStateManager.P1First == _utilityInfo.ActivatedByPlayer1;
        bool cardNotFromHand = !_placingPlayer.Hand.CardObjs.Contains(selectedCard.gameObject);
        bool invalidCardType = cardType == ICard.CardType.Utility;
        CardSlot[] opponentBoard = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.GameBoard.Player2Board : Locator.Instance.GameBoard.Player1Board;
        CardSlot chosenCardSlot = FindValidCardSlot(opponentBoard, cardType);
        
        if (cardNotFromHand || invalidCardType || chosenCardSlot == null) return;
        
        // Add the card to the enemy spell
        CardManager cardManager = Locator.Instance.CardManager;
        cardManager.RemoveCardFromPlayer(selectedCard.gameObject, _utilityInfo.ActivatedByPlayer1);
        cardManager.InstantiateCardToSlot(selectedCard.CardData, chosenCardSlot, cardType != ICard.CardType.Core, _utilityInfo.ActivatedByPlayer1);
        cardManager.DiscardCard(selectedCard);

        //Finish
        Locator.Instance.GameStateManager.UpdateState();
        _placingPlayer.Interaction.PickupDisabled = false;
        _placingPlayer.Interaction.OnCardSelected -= OnCardSelected;
        _utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(_utilityInfo);
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
        bool isP1ExtendedTurn = Locator.Instance.GameStateManager.CurrentStateIndex == (int)GameStateManager.GameStateIndex.Player1ExtendedTurn;
        bool isP2ExtendedTurn = Locator.Instance.GameStateManager.CurrentStateIndex == (int)GameStateManager.GameStateIndex.Player2ExtendedTurn;
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
