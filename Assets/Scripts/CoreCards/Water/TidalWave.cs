using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tidal Wave", fileName = "Tidal_Wave")]
public class TidalWave : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        CardValues combinedValues;
        combinedValues.power = finalValues.fireValues.power + finalValues.electricityValues.power;
        combinedValues.special = finalValues.fireValues.special + finalValues.electricityValues.special;

        finalValues.waterValues.power += combinedValues.power;
        finalValues.waterValues.special += combinedValues.special;

        finalValues.fireValues.power = 0;
        finalValues.fireValues.special = 0;
        finalValues.electricityValues.power = 0;
        finalValues.electricityValues.special = 0;

        return finalValues;
    }
}
