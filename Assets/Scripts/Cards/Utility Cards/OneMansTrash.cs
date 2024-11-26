using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/One Mans Trash", fileName = "One_Mans_Trash")]
public class OneMansTrash : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        Deck tempDeck = utilityInfo.Player1.Hand;

        utilityInfo.Player1.Hand = utilityInfo.Player2.Hand;
        utilityInfo.Player2.Hand = tempDeck;

        utilityInfo.CardManager.SwapPlayerCards();

        OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1 ? utilityInfo.Player1.Hand : utilityInfo.Player2.Hand);
    }
}
