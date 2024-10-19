using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Sabotage", fileName = "Sabotage")]
public class Sabotage : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityManager.UtilityInfo utilityInfo)
    {

    }
}
