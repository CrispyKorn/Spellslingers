using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Sabotage", fileName = "Sabotage")]
public class Sabotage : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
