using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility Card/Refresh", fileName = "Refresh")]
public class Refresh : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private List<PlayCard> _selectedCards = new();
    private int _numOfCardsToTake = 3;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _selectedCards.Clear();
        _numOfCardsToTake = Mathf.Min(3, _utilityInfo.Player1.Hand.Cards.Count);
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player1 : _utilityInfo.Player2;

        _placingPlayer.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        if (_selectedCards.Contains(selectedCard) || !_placingPlayer.Hand.Cards.Contains(selectedCard.CardData)) return;
        
        _selectedCards.Add(selectedCard);

        if (_selectedCards.Count == _numOfCardsToTake)
        {
            foreach (PlayCard card in _selectedCards)
            {
                List<GameObject> playerCards = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.CardManager.Player1Cards : _utilityInfo.CardManager.Player2Cards;
                ICard.CardType cardType = card.CardData.Type;

                _utilityInfo.CardManager.DiscardCard(_placingPlayer.Hand, playerCards, card);

                // Refund Player
                Deck drawDeck = null;
                switch (cardType)
                {
                    case ICard.CardType.Core: drawDeck = _utilityInfo.CardManager.CoreDeck; break;
                    case ICard.CardType.Offence: drawDeck = _utilityInfo.CardManager.OffenceDeck; break;
                    case ICard.CardType.Defence: drawDeck = _utilityInfo.CardManager.DefenceDeck; break;
                    case ICard.CardType.Utility: drawDeck = _utilityInfo.CardManager.UtilityDeck; break;
                }

                ICard newCard = _utilityInfo.CardManager.DrawOne(drawDeck);
                _placingPlayer.Hand.Cards.Add(newCard);

                bool isLocalPlayerCards = _utilityInfo.ActivatedByPlayer1;
                ulong player2ClientId = _utilityInfo.CardManager.NetworkManager.ConnectedClients[1].ClientId;
                _ = _utilityInfo.CardManager.InstantiateCards(new List<ICard>() { newCard }, isLocalPlayerCards, player2ClientId, _utilityInfo.Player1, _utilityInfo.Player2);
            }

            if (_utilityInfo.ActivatedByPlayer1) _utilityInfo.Player1.OnCardSelected -= OnCardSelected;
            else _utilityInfo.Player2.OnCardSelected -= OnCardSelected;

            OnCardEffectComplete?.Invoke(this, _placingPlayer.Hand);
        }
    }
}
