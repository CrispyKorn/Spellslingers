using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Smooth Talker", fileName = "Smooth_Talker")]
public class SmoothTalker : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;
    UtilityManager.UtilityInfo utilityInfo;
    Player placingPlayer, opponent;
    PlayCard selectedCard;

    public override void ApplyEffect(UtilityManager.UtilityInfo _utilityInfo)
    {
        utilityInfo = _utilityInfo;
        placingPlayer = utilityInfo.activatedByPlayer1 ? utilityInfo.player1 : utilityInfo.player2;
        opponent = utilityInfo.activatedByPlayer1 ? utilityInfo.player2 : utilityInfo.player1;
        opponent.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        List<GameObject> placingPlayerCards = utilityInfo.activatedByPlayer1 ? utilityInfo.cardManager.Player1Cards : utilityInfo.cardManager.Player2Cards;
        List<GameObject> opponentCards = utilityInfo.activatedByPlayer1 ? utilityInfo.cardManager.Player2Cards : utilityInfo.cardManager.Player1Cards;

        if (!opponentCards.Contains(selectedCard.gameObject)) return;

        utilityInfo.cardManager.DiscardCard(opponent.hand, opponentCards, selectedCard);

        List<ICard> cardToAdd = new List<ICard> { selectedCard.cardData };
        ulong player2ClientId = utilityInfo.cardManager.NetworkManager.ConnectedClients[1].ClientId;
        _ = utilityInfo.cardManager.InstantiateCards(cardToAdd, utilityInfo.activatedByPlayer1, player2ClientId, utilityInfo.player1, utilityInfo.player2);

        placingPlayer.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, placingPlayer.hand);
    }
}
