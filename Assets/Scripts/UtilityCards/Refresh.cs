using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(menuName = "Utility Card/Refresh", fileName = "Refresh")]
public class Refresh : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;
    UtilityManager.UtilityInfo utilityInfo;
    List<PlayCard> selectedCards;
    int numOfCardsToTake = 3;
    Player placingPlayer;

    public override void ApplyEffect(UtilityManager.UtilityInfo _utilityInfo)
    {
        utilityInfo = _utilityInfo;
        selectedCards = new List<PlayCard>();
        numOfCardsToTake = Mathf.Min(3, utilityInfo.player1.hand.cards.Count);
        placingPlayer = utilityInfo.activatedByPlayer1 ? utilityInfo.player1 : utilityInfo.player2;

        placingPlayer.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        if (selectedCards.Contains(selectedCard) || !placingPlayer.hand.cards.Contains(selectedCard.cardData)) return;
        
        selectedCards.Add(selectedCard);

        if (selectedCards.Count == numOfCardsToTake)
        {
            foreach (PlayCard card in selectedCards)
            {
                List<GameObject> playerCards = utilityInfo.activatedByPlayer1 ? utilityInfo.cardManager.Player1Cards : utilityInfo.cardManager.Player2Cards;
                ICard.CardType cardType = card.cardData.Type;

                utilityInfo.cardManager.DiscardCard(placingPlayer.hand, playerCards, card);

                //Refund Player
                Deck drawDeck = null;
                switch (cardType)
                {
                    case ICard.CardType.Core: drawDeck = utilityInfo.cardManager.CoreDeck; break;
                    case ICard.CardType.Offence: drawDeck = utilityInfo.cardManager.OffenceDeck; break;
                    case ICard.CardType.Defence: drawDeck = utilityInfo.cardManager.DefenceDeck; break;
                    case ICard.CardType.Utility: drawDeck = utilityInfo.cardManager.UtilityDeck; break;
                }

                ICard newCard = utilityInfo.cardManager.Draw(drawDeck, 1)[0];
                placingPlayer.hand.cards.Add(newCard);

                bool isLocalPlayerCards = utilityInfo.activatedByPlayer1;
                ulong player2ClientId = utilityInfo.cardManager.NetworkManager.ConnectedClients[1].ClientId;
                _ = utilityInfo.cardManager.InstantiateCards(new List<ICard>() { newCard }, isLocalPlayerCards, player2ClientId, utilityInfo.player1, utilityInfo.player2);
            }

            if (utilityInfo.activatedByPlayer1) utilityInfo.player1.OnCardSelected -= OnCardSelected;
            else utilityInfo.player2.OnCardSelected -= OnCardSelected;

            OnCardEffectComplete?.Invoke(this, placingPlayer.hand);
        }
    }
}
