using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Chaotic Thunderstorm", fileName = "Chaotic_Thunderstorm")]
public class ChaoticThunderstorm : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Electricity) finalValues.electricityValues.power += card.values.power;
            else
            {
                finalValues.electricityValues.power = 0;
                break;
            }
        }

        return finalValues;
    }
}
