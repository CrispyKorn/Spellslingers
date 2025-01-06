using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility Card/Refresh", fileName = "Refresh")]
public class Refresh : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private List<PlayCard> _selectedCards = new();
    private int _numOfCardsToTake = 3;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _selectedCards.Clear();
        _numOfCardsToTake = Mathf.Min(3, _utilityInfo.Player1.Hand.Size);
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player1 : _utilityInfo.Player2;

        _placingPlayer.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        // Check for invalid selections
        if (_selectedCards.Contains(selectedCard) || !_placingPlayer.Hand.CardObjs.Contains(selectedCard.gameObject)) return;
        
        // Add the selected card
        _selectedCards.Add(selectedCard);

        // Handle activation
        if (_selectedCards.Count == _numOfCardsToTake)
        {
            Hand playerHand = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player1.Hand : _utilityInfo.Player2.Hand;

            foreach (PlayCard card in _selectedCards)
            {
                // Discard
                _utilityInfo.CardManager.DiscardCard(card);

                // Refund Player
                Deck drawDeck = null;
                switch (card.CardData.Type)
                {
                    case ICard.CardType.Core: drawDeck = _utilityInfo.CardManager.CoreDeck; break;
                    case ICard.CardType.Offence: drawDeck = _utilityInfo.CardManager.OffenceDeck; break;
                    case ICard.CardType.Defence: drawDeck = _utilityInfo.CardManager.DefenceDeck; break;
                    case ICard.CardType.Utility: drawDeck = _utilityInfo.CardManager.UtilityDeck; break;
                }

                ICard newCard = _utilityInfo.CardManager.DrawOne(drawDeck);
                _ = _utilityInfo.CardManager.InstantiateCards(new List<ICard>() { newCard }, _utilityInfo.ActivatedByPlayer1);
            }

            // Finish
            _placingPlayer.OnCardSelected -= OnCardSelected;
            OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, true);
        }
    }
}
