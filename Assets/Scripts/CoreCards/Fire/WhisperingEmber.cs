using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Whispering Ember", fileName = "Whispering_Ember")]
public class WhisperingEmber : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        finalValues.fireValues.power -= values.power;

        foreach (Card card in _peripheralCards)
        {
            if (values.power > 0) values.power--;
        }

        finalValues.fireValues.power += values.power;

        return finalValues;
    }
}
