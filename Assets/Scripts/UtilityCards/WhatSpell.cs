using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/What Spell", fileName = "What_Spell")]
public class WhatSpell : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityManager.UtilityInfo utilityInfo)
    {

    }
}
