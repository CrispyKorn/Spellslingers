using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Roll The Dice", fileName = "Roll_The_Dice")]
public class RollTheDice : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        // Setup required data
        CardManager cardManager = Locator.Instance.CardManager;
        CardSlot[] playerBoard = utilityInfo.ActivatedByPlayer1 ? Locator.Instance.GameBoard.Player1Board : Locator.Instance.GameBoard.Player2Board;
        CardSlot coreSlot = playerBoard[(int)GameBoard.Slot.CoreSlot];
        bool coreCardIsFaceUp = false;
        ICard newCoreCard = cardManager.DrawOne(cardManager.CoreDeck);

        // Add new card
        if (coreSlot.HasCard) 
        {
            coreCardIsFaceUp = coreSlot.Card.GetComponent<PlayCard>().IsFaceUp;
            cardManager.DiscardCard(coreSlot.TakeCard().GetComponent<PlayCard>());
        }
        
        cardManager.InstantiateCardToSlot(newCoreCard, coreSlot, coreCardIsFaceUp, !utilityInfo.ActivatedByPlayer1);

        // Update game state
        Locator.Instance.GameStateManager.UpdateState();

        //Finish
        utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(utilityInfo);
    }
}
