using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/One Mans Trash", fileName = "One_Mans_Trash")]
public class OneMansTrash : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        utilityInfo.CardManager.SwapPlayerCards();

        OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1, true);
    }
}
