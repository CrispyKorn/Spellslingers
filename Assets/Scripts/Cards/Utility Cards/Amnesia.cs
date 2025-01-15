using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Amnesia", fileName = "Amnesia")]
public class Amnesia : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        CardManager cardManager = Locator.Instance.CardManager;
        Hand affectedHand = utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayerManager.Player2.Hand : Locator.Instance.PlayerManager.Player1.Hand;
        var coreNum = 0;
        var offenceNum = 0;
        var defenceNum = 0;
        var utilityNum = 0;

        // Count the number of each type of card in the affected player's hand
        for (var i = affectedHand.Size; i >= 0; i--)
        {
            var playCard = affectedHand.CardObjs[i].GetComponent<PlayCard>();

            switch (playCard.CardData.Type)
            {
                case ICard.CardType.Core: coreNum++; break;
                case ICard.CardType.Offence: offenceNum++; break;
                case ICard.CardType.Defence: defenceNum++; break;
                case ICard.CardType.Utility: utilityNum++; break;
            }

            cardManager.RemoveCardFromPlayer(playCard.gameObject, !utilityInfo.ActivatedByPlayer1);
            cardManager.DiscardCard(playCard);
        }

        // Refund Cards
        List<ICard> newCards = new();

        foreach (ICard card in cardManager.Draw(cardManager.CoreDeck, coreNum)) newCards.Add(card);
        foreach (ICard card in cardManager.Draw(cardManager.OffenceDeck, offenceNum)) newCards.Add(card);
        foreach (ICard card in cardManager.Draw(cardManager.DefenceDeck, defenceNum)) newCards.Add(card);
        foreach (ICard card in cardManager.Draw(cardManager.UtilityDeck, utilityNum)) newCards.Add(card);

        _ = cardManager.InstantiateCards(newCards, !utilityInfo.ActivatedByPlayer1);

        // Finish
        utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(utilityInfo);
    }
}
