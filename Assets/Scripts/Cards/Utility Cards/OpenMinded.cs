using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Open Minded", fileName = "Open_Minded")]
public class OpenMinded : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete { add { } remove { } }

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {

    }
}
