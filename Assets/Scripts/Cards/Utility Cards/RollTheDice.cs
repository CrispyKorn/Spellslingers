using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Roll The Dice", fileName = "Roll_The_Dice")]
public class RollTheDice : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
