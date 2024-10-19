using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Radiant Lightshow", fileName = "Radiant_Lightshow")]
public class RadiantLightshow : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        bool usedFire = false;
        bool usedWater = false;

        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Fire) usedFire = true;
            if (card.element == CardElement.Water) usedWater = true;
        }

        int additionalSpellTypeNum = 0;
        if (usedFire) additionalSpellTypeNum++;
        if (usedWater) additionalSpellTypeNum++;

        finalValues.electricityValues.power += additionalSpellTypeNum;

        return finalValues;
    }
}
