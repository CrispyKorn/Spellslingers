using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tumbling Avalanche", fileName = "Tumbling_Avalanche")]
public class TumblingAvalanche : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        if (_peripheralCards.Length < 3) finalValues.waterValues.power -= values.power;

        return finalValues;
    }
}
