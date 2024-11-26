using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Amnesia", fileName = "Amnesia")]
public class Amnesia : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        Deck affectedHand = utilityInfo.ActivatedByPlayer1 ? utilityInfo.Player2.Hand : utilityInfo.Player1.Hand;
        List<GameObject> playerCards = utilityInfo.ActivatedByPlayer1 ? utilityInfo.CardManager.Player2Cards : utilityInfo.CardManager.Player1Cards;
        var coreNum = 0;
        var offenceNum = 0;
        var defenceNum = 0;
        var utilityNum = 0;

        for (var i = 0; i < playerCards.Count; i++)
        {
            var playCard = playerCards[i].GetComponent<PlayCard>();

            switch (playCard.CardData.Type)
            {
                case ICard.CardType.Core: coreNum++; break;
                case ICard.CardType.Offence: offenceNum++; break;
                case ICard.CardType.Defence: defenceNum++; break;
                case ICard.CardType.Utility: utilityNum++; break;
            }

            utilityInfo.CardManager.DiscardCard(affectedHand, playerCards, playCard);
        }

        // Refund Cards
        List<ICard> newCards = new();

        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.CoreDeck, coreNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.OffenceDeck, offenceNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.DefenceDeck, defenceNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.CardManager.Draw(utilityInfo.CardManager.UtilityDeck, utilityNum)) newCards.Add(card);

        bool isLocalPlayerCards = !utilityInfo.ActivatedByPlayer1;
        ulong player2ClientId = utilityInfo.CardManager.NetworkManager.ConnectedClients[1].ClientId;
        _ = utilityInfo.CardManager.InstantiateCards(newCards, isLocalPlayerCards, player2ClientId, utilityInfo.Player1, utilityInfo.Player2);

        OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1 ? utilityInfo.Player1.Hand : utilityInfo.Player2.Hand);
    }
}
