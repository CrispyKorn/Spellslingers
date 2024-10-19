using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Flickering Lightningbugs", fileName = "Flickering_Lightningbugs")]
public class FlickeringLightningbugs : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        finalValues.electricityValues.power -= values.power;

        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Fire || card.element == CardElement.Water)
            {
                if (values.power > 0) values.power--;
            }
        }

        finalValues.electricityValues.power += values.power;

        return finalValues;
    }
}
