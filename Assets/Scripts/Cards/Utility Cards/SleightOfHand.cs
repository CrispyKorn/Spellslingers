using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Utility Card/Sleight of Hand", fileName = "Sleight_Of_Hand")]
public class SleightOfHand : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private PlayerManager _playerManager;
    private CardManager _cardManager;
    private Player _placingPlayer;
    private PlayCard _selectedOpponentCard;
    private PlayCard _selectedPlayerCard;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _playerManager = Locator.Instance.PlayerManager;
        _cardManager = Locator.Instance.CardManager;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player1 : _playerManager.Player2;
        _selectedPlayerCard = null;
        _selectedOpponentCard = null;

        _cardManager.SetCardHighlights(true, true);
        _cardManager.SetCardHighlights(true, false);

        _placingPlayer.Interaction.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        Hand placingPlayerHand = _placingPlayer.Hand;
        Hand opponentHand = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player2.Hand : _playerManager.Player1.Hand;
        
        if (placingPlayerHand.CardObjs.Contains(selectedCard.gameObject))
        {
            _selectedPlayerCard = selectedCard;

            if (_selectedOpponentCard == null) UpdateCardHighlights(true);
            
        }
        else if (opponentHand.CardObjs.Contains(selectedCard.gameObject)) 
        {
            _selectedOpponentCard = selectedCard;

            if (_selectedPlayerCard == null) UpdateCardHighlights(false);
        }

        // Check for incomplete selection
        if (_selectedPlayerCard == null || _selectedOpponentCard == null) return;

        //Both cards selected, swap!
        _cardManager.RemoveCardFromPlayer(_selectedPlayerCard.gameObject, _utilityInfo.ActivatedByPlayer1);
        _cardManager.RemoveCardFromPlayer(_selectedOpponentCard.gameObject, !_utilityInfo.ActivatedByPlayer1);
        _cardManager.GiveCardToPlayer(_selectedOpponentCard.gameObject, _utilityInfo.ActivatedByPlayer1);
        _cardManager.GiveCardToPlayer(_selectedPlayerCard.gameObject, !_utilityInfo.ActivatedByPlayer1);

        // Finish
        _cardManager.SetCardHighlights(false, true);
        _cardManager.SetCardHighlights(false, false);
        _placingPlayer.Interaction.OnCardSelected -= OnCardSelected;
        _utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(_utilityInfo);
    }

    private void UpdateCardHighlights(bool placingPlayerCardSelected)
    {
        ICard.CardType selectedCardType = placingPlayerCardSelected ? _selectedPlayerCard.CardData.Type : _selectedOpponentCard.CardData.Type;
        bool player1CardsSelected = _utilityInfo.ActivatedByPlayer1 == placingPlayerCardSelected;

        _cardManager.SetCardHighlights(false, player1CardsSelected);
        _cardManager.SetCardHighlights(true, !player1CardsSelected, selectedCardType);
    }
}
