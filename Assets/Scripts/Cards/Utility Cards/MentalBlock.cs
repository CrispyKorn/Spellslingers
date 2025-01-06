using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Mental Block", fileName = "Mental_Block")]
public class MentalBlock : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1, true);
    }
}
