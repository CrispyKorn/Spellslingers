using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tesla Coil", fileName = "Tesla_Coil")]
public class TeslaCoil : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        finalValues.electricityValues.power -= values.power;

        foreach (Card card in _peripheralCards)
        {
            if (values.power > 0) values.power--;
        }

        finalValues.electricityValues.power += values.power;

        return finalValues;
    }
}
