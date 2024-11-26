using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Mind Control", fileName = "Mind_Control")]
public class MindControl : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
