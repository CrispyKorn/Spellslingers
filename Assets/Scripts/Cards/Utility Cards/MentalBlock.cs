using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Mental Block", fileName = "Mental_Block")]
public class MentalBlock : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(utilityInfo);
    }
}
