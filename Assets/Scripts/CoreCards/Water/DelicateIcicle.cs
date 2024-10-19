using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Delicate Icicle", fileName = "Delicate_Icicle")]
public class DelicateIcicle : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        finalValues.waterValues.power -= values.power;

        foreach (Card card in _peripheralCards)
        {
            if (values.power > 0) values.power--;
        }

        finalValues.waterValues.power += values.power;

        return finalValues;
    }
}
