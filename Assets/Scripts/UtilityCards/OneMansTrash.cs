using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/One Mans Trash", fileName = "One_Mans_Trash")]
public class OneMansTrash : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityManager.UtilityInfo utilityInfo)
    {
        Deck tempDeck = utilityInfo.player1.hand;

        utilityInfo.player1.hand = utilityInfo.player2.hand;
        utilityInfo.player2.hand = tempDeck;

        utilityInfo.cardManager.SwapPlayerCards();

        OnCardEffectComplete?.Invoke(this, utilityInfo.activatedByPlayer1 ? utilityInfo.player1.hand : utilityInfo.player2.hand);
    }
}
