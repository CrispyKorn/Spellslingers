using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Spewing Volcano", fileName = "Spewing_Volcano")]
public class SpewingVolcano : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        CardValues combinedValues;
        combinedValues.power = finalValues.waterValues.power + finalValues.electricityValues.power;
        combinedValues.special = finalValues.waterValues.special + finalValues.electricityValues.special;

        finalValues.fireValues.power += combinedValues.power;
        finalValues.fireValues.special += combinedValues.special;

        finalValues.waterValues.power = 0;
        finalValues.waterValues.special = 0;
        finalValues.electricityValues.power = 0;
        finalValues.electricityValues.special = 0;

        return finalValues;
    }
}
