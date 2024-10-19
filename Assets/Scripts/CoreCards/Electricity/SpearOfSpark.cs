using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Spear of Spark", fileName = "Spear_Of_Spark")]
public class SpearOfSpark : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Electricity) finalValues.electricityValues.power++;
        }

        return finalValues;
    }
}
