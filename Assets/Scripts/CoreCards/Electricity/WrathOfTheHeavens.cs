using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Wrath of the Heavens", fileName = "Wrath_Of_The_Heavens")]
public class WrathOfTheHeavens : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        if (_peripheralCards.Length < 3) finalValues.electricityValues.power -= values.power;

        return finalValues;
    }
}
