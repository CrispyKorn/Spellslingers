using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Rushing Waterfall", fileName = "Rushing_Waterfall")]
public class RushingWaterfall : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Water) finalValues.waterValues.power++;
        }

        return finalValues;
    }
}
