using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Amnesia", fileName = "Amnesia")]
public class Amnesia : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        Hand affectedHand = utilityInfo.ActivatedByPlayer1 ? utilityInfo.Player2.Hand : utilityInfo.Player1.Hand;
        var coreNum = 0;
        var offenceNum = 0;
        var defenceNum = 0;
        var utilityNum = 0;
        int affectedHandSize = affectedHand.Size;

        // Count the number of each type of card in the affected player's hand
        for (var i = 0; i < affectedHandSize; i++)
        {
            var playCard = affectedHand.CardObjs[i].GetComponent<PlayCard>();

            switch (playCard.CardData.Type)
            {
                case ICard.CardType.Core: coreNum++; break;
                case ICard.CardType.Offence: offenceNum++; break;
                case ICard.CardType.Defence: defenceNum++; break;
                case ICard.CardType.Utility: utilityNum++; break;
            }

            utilityInfo.CardManager.DiscardCard(playCard);
        }

        // Refund Cards
        List<ICard> newCards = new();

        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.CoreDeck, coreNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.OffenceDeck, offenceNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.DefenceDeck, defenceNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.UtilityDeck, utilityNum)) newCards.Add(card);

        _ = utilityInfo.CardManager.InstantiateCards(newCards, utilityInfo.ActivatedByPlayer1);

        // Finish
        OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1, true);
    }
}
