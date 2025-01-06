using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/What Spell", fileName = "What_Spell")]
public class WhatSpell : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
