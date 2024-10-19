using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Piercing Sunbeams", fileName = "Piercing_Sunbeams")]
public class PiercingSunbeams : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        finalValues.fireValues.power -= values.power;

        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Water || card.element == CardElement.Electricity)
            {
                if (values.power > 0) values.power--;
            }
        }

        finalValues.fireValues.power += values.power;

        return finalValues;
    }
}
