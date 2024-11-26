using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Mental Block", fileName = "Mental_Block")]
public class MentalBlock : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
