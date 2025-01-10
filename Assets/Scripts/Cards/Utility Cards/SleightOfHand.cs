using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Utility Card/Sleight of Hand", fileName = "Sleight_Of_Hand")]
public class SleightOfHand : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private PlayerManager _playerManager;
    private Player _placingPlayer;
    private PlayCard _selectedOpponentCard;
    private PlayCard _selectedPlayerCard;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _playerManager = Locator.Instance.PlayerManager;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player1 : _playerManager.Player2;
        _placingPlayer.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        Hand placingPlayerHand = _placingPlayer.Hand;
        Hand opponentHand = _utilityInfo.ActivatedByPlayer1 ? _playerManager.Player2.Hand : _playerManager.Player1.Hand;
        
        if (placingPlayerHand.CardObjs.Contains(selectedCard.gameObject)) _selectedPlayerCard = selectedCard;
        else if (opponentHand.CardObjs.Contains(selectedCard.gameObject)) _selectedOpponentCard = selectedCard;

        // Check for incomplete selection
        if (_selectedPlayerCard == null || _selectedOpponentCard == null) return;

        //Both cards selected, swap!
        CardManager cardManager = Locator.Instance.CardManager;
        cardManager.RemoveCardFromPlayer(_selectedPlayerCard.gameObject, _utilityInfo.ActivatedByPlayer1);
        cardManager.RemoveCardFromPlayer(_selectedOpponentCard.gameObject, !_utilityInfo.ActivatedByPlayer1);
        cardManager.GiveCardToPlayer(_selectedOpponentCard.gameObject, _utilityInfo.ActivatedByPlayer1);
        cardManager.GiveCardToPlayer(_selectedPlayerCard.gameObject, !_utilityInfo.ActivatedByPlayer1);

        _placingPlayer.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, true);
    }
}
