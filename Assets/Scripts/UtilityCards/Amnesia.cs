using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Amnesia", fileName = "Amnesia")]
public class Amnesia : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityManager.UtilityInfo utilityInfo)
    {
        Deck affectedHand = utilityInfo.activatedByPlayer1 ? utilityInfo.player2.hand : utilityInfo.player1.hand;
        List<GameObject> playerCards = utilityInfo.activatedByPlayer1 ? utilityInfo.cardManager.Player2Cards : utilityInfo.cardManager.Player1Cards;

        int coreNum = 0, offenceNum = 0, defenceNum = 0, utilityNum = 0;

        for (int i = 0; i < playerCards.Count; i++)
        {
            PlayCard playCard = playerCards[i].GetComponent<PlayCard>();

            switch (playCard.cardData.Type)
            {
                case ICard.CardType.Core: coreNum++; break;
                case ICard.CardType.Offence: offenceNum++; break;
                case ICard.CardType.Defence: defenceNum++; break;
                case ICard.CardType.Utility: utilityNum++; break;
            }

            utilityInfo.cardManager.DiscardCard(affectedHand, playerCards, playCard);
        }

        //Refund Cards
        List<ICard> newCards = new List<ICard>();

        foreach (ICard card in utilityInfo.cardManager.Draw(utilityInfo.cardManager.CoreDeck, coreNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.cardManager.Draw(utilityInfo.cardManager.OffenceDeck, offenceNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.cardManager.Draw(utilityInfo.cardManager.DefenceDeck, defenceNum)) newCards.Add(card);
        foreach (ICard card in utilityInfo.cardManager.Draw(utilityInfo.cardManager.UtilityDeck, utilityNum)) newCards.Add(card);

        bool isLocalPlayerCards = !utilityInfo.activatedByPlayer1;
        ulong player2ClientId = utilityInfo.cardManager.NetworkManager.ConnectedClients[1].ClientId;
        _ = utilityInfo.cardManager.InstantiateCards(newCards, isLocalPlayerCards, player2ClientId, utilityInfo.player1, utilityInfo.player2);

        OnCardEffectComplete?.Invoke(this, utilityInfo.activatedByPlayer1 ? utilityInfo.player1.hand : utilityInfo.player2.hand);
    }
}
