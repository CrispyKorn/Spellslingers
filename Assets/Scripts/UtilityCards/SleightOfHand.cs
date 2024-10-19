using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Sleight of Hand", fileName = "Sleight_Of_Hand")]
public class SleightOfHand : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;
    UtilityManager.UtilityInfo utilityInfo;
    Player placingPlayer, opponent;
    PlayCard selectedOpponentCard, selectedPlayerCard;

    public override void ApplyEffect(UtilityManager.UtilityInfo _utilityInfo)
    {
        utilityInfo = _utilityInfo;
        placingPlayer = utilityInfo.activatedByPlayer1 ? utilityInfo.player1 : utilityInfo.player2;
        opponent = utilityInfo.activatedByPlayer1 ? utilityInfo.player2 : utilityInfo.player1;
        placingPlayer.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        if (utilityInfo.activatedByPlayer1)
        {
            if (utilityInfo.cardManager.Player1Cards.Contains(selectedCard.gameObject)) selectedPlayerCard = selectedCard;
        }
        else
        {
            if (utilityInfo.cardManager.Player2Cards.Contains(selectedCard.gameObject)) selectedOpponentCard = selectedCard;
        }

        if (selectedPlayerCard == null || selectedOpponentCard == null) return;

        //Both Cards Selected, Swap!
        List<GameObject> placingPlayerCards = utilityInfo.activatedByPlayer1 ? utilityInfo.cardManager.Player1Cards : utilityInfo.cardManager.Player2Cards;
        List<GameObject> opponentCards = utilityInfo.activatedByPlayer1 ? utilityInfo.cardManager.Player2Cards : utilityInfo.cardManager.Player1Cards;

        utilityInfo.cardManager.DiscardCard(placingPlayer.hand, placingPlayerCards, selectedPlayerCard);
        utilityInfo.cardManager.DiscardCard(opponent.hand, opponentCards, selectedOpponentCard);

        List<ICard> cardToAdd = new List<ICard> { selectedOpponentCard.cardData };
        ulong player2ClientId = utilityInfo.cardManager.NetworkManager.ConnectedClients[1].ClientId;
        _ = utilityInfo.cardManager.InstantiateCards(cardToAdd, utilityInfo.activatedByPlayer1, player2ClientId, utilityInfo.player1, utilityInfo.player2);
        cardToAdd[0] = selectedPlayerCard.cardData;
        _ = utilityInfo.cardManager.InstantiateCards(cardToAdd, !utilityInfo.activatedByPlayer1, player2ClientId, utilityInfo.player1, utilityInfo.player2);


        if (utilityInfo.activatedByPlayer1) utilityInfo.player1.OnCardSelected -= OnCardSelected;
        else utilityInfo.player2.OnCardSelected -= OnCardSelected;

        OnCardEffectComplete?.Invoke(this, placingPlayer.hand);
    }
}
