using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Smooth Talker", fileName = "Smooth_Talker")]
public class SmoothTalker : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private Player _placingPlayer;
    private Player _opponent;
    private PlayCard _selectedCard;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player1 : _utilityInfo.Player2;
        _opponent = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player2 : _utilityInfo.Player1;
        _opponent.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        List<GameObject> placingPlayerCards = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.CardManager.Player1Cards : _utilityInfo.CardManager.Player2Cards;
        List<GameObject> opponentCards = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.CardManager.Player2Cards : _utilityInfo.CardManager.Player1Cards;

        if (!opponentCards.Contains(selectedCard.gameObject)) return;

        _utilityInfo.CardManager.DiscardCard(_opponent.Hand, opponentCards, selectedCard);

        var cardToAdd = new List<ICard> { selectedCard.CardData };
        ulong player2ClientId = _utilityInfo.CardManager.NetworkManager.ConnectedClients[1].ClientId;
        _ = _utilityInfo.CardManager.InstantiateCards(cardToAdd, _utilityInfo.ActivatedByPlayer1, player2ClientId, _utilityInfo.Player1, _utilityInfo.Player2);

        _placingPlayer.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, _placingPlayer.Hand);
    }
}
