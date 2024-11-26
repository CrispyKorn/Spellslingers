using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Sleight of Hand", fileName = "Sleight_Of_Hand")]
public class SleightOfHand : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private Player _placingPlayer;
    private Player _opponent;
    private PlayCard _selectedOpponentCard;
    private PlayCard _selectedPlayerCard;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player1 : _utilityInfo.Player2;
        _opponent = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player2 : _utilityInfo.Player1;
        _placingPlayer.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        if (_utilityInfo.ActivatedByPlayer1)
        {
            if (_utilityInfo.CardManager.Player1Cards.Contains(selectedCard.gameObject)) _selectedPlayerCard = selectedCard;
        }
        else
        {
            if (_utilityInfo.CardManager.Player2Cards.Contains(selectedCard.gameObject)) _selectedOpponentCard = selectedCard;
        }

        if (_selectedPlayerCard == null || _selectedOpponentCard == null) return;

        //Both Cards Selected, Swap!
        List<GameObject> placingPlayerCards = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.CardManager.Player1Cards : _utilityInfo.CardManager.Player2Cards;
        List<GameObject> opponentCards = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.CardManager.Player2Cards : _utilityInfo.CardManager.Player1Cards;

        _utilityInfo.CardManager.DiscardCard(_placingPlayer.Hand, placingPlayerCards, _selectedPlayerCard);
        _utilityInfo.CardManager.DiscardCard(_opponent.Hand, opponentCards, _selectedOpponentCard);

        var cardToAdd = new List<ICard> { _selectedOpponentCard.CardData };
        ulong player2ClientId = _utilityInfo.CardManager.NetworkManager.ConnectedClients[1].ClientId;
        _ = _utilityInfo.CardManager.InstantiateCards(cardToAdd, _utilityInfo.ActivatedByPlayer1, player2ClientId, _utilityInfo.Player1, _utilityInfo.Player2);
        cardToAdd[0] = _selectedPlayerCard.CardData;
        _ = _utilityInfo.CardManager.InstantiateCards(cardToAdd, !_utilityInfo.ActivatedByPlayer1, player2ClientId, _utilityInfo.Player1, _utilityInfo.Player2);

        if (_utilityInfo.ActivatedByPlayer1) _utilityInfo.Player1.OnCardSelected -= OnCardSelected;
        else _utilityInfo.Player2.OnCardSelected -= OnCardSelected;

        OnCardEffectComplete?.Invoke(this, _placingPlayer.Hand);
    }
}
