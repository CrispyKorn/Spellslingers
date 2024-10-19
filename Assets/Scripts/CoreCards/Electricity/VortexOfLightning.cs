using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Vortex of Lightning", fileName = "Vortex_Of_Lightning")]
public class VortexOfLightning : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        CardValues combinedValues;
        combinedValues.power = finalValues.fireValues.power + finalValues.waterValues.power;
        combinedValues.special = finalValues.fireValues.special + finalValues.waterValues.special;

        finalValues.electricityValues.power += combinedValues.power;
        finalValues.electricityValues.special += combinedValues.special;

        finalValues.fireValues.power = 0;
        finalValues.fireValues.special = 0;
        finalValues.waterValues.power = 0;
        finalValues.waterValues.special = 0;

        return finalValues;
    }
}
