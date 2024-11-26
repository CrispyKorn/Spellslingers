using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Sabotage", fileName = "Sabotage")]
public class Sabotage : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
