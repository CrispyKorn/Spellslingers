using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Single Minded", fileName = "Single_Minded")]
public class SingleMinded : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
