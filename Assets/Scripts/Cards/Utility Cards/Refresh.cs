using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility Card/Refresh", fileName = "Refresh")]
public class Refresh : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private PlayerManager _playerManager;
    private CardManager _cardManager;
    private List<PlayCard> _selectedCards = new();
    private int _numOfCardsToTake = 3;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _playerManager = Locator.Instance.PlayerManager;
        _cardManager = Locator.Instance.CardManager;
        _selectedCards.Clear();
        _numOfCardsToTake = Mathf.Min(3, _playerManager.Player1.Hand.Size);
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player1 : _playerManager.Player2;

        _placingPlayer.Interaction.PickupDisabled = true;
        _cardManager.SetCardHighlights(true, _utilityInfo.ActivatedByPlayer1);

        _placingPlayer.Interaction.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        // Check for invalid selections
        if (_selectedCards.Contains(selectedCard) || !_placingPlayer.Hand.CardObjs.Contains(selectedCard.gameObject)) return;
        
        // Add the selected card
        _selectedCards.Add(selectedCard);
        selectedCard.SetDraggableRpc(false);

        // Handle activation
        if (_selectedCards.Count == _numOfCardsToTake)
        {
            foreach (PlayCard card in _selectedCards)
            {
                // Refund Player
                Deck drawDeck = null;
                switch (card.CardData.Type)
                {
                    case ICard.CardType.Core: drawDeck = _cardManager.CoreDeck; break;
                    case ICard.CardType.Offence: drawDeck = _cardManager.OffenceDeck; break;
                    case ICard.CardType.Defence: drawDeck = _cardManager.DefenceDeck; break;
                    case ICard.CardType.Utility: drawDeck = _cardManager.UtilityDeck; break;
                }

                ICard newCard = _cardManager.DrawOne(drawDeck);
                _ = _cardManager.InstantiateCards(new List<ICard>() { newCard }, _utilityInfo.ActivatedByPlayer1);

                // Discard
                _cardManager.RemoveCardFromPlayer(card.gameObject, _utilityInfo.ActivatedByPlayer1);
                _cardManager.DiscardCard(card);
            }

            // Finish
            _placingPlayer.Interaction.PickupDisabled = false;
            _cardManager.SetCardHighlights(false, _utilityInfo.ActivatedByPlayer1);
            _placingPlayer.Interaction.OnCardSelected -= OnCardSelected;
            _utilityInfo.Successful = true;
            OnCardEffectComplete?.Invoke(_utilityInfo);
        }
    }
}
