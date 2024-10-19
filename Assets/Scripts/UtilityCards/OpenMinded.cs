using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Open Minded", fileName = "Open_Minded")]
public class OpenMinded : UtilityCard
{
    public override event Action<UtilityCard, Deck> OnCardEffectComplete;

    public override void ApplyEffect(UtilityManager.UtilityInfo utilityInfo)
    {

    }
}
