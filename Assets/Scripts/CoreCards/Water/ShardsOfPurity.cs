using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Shards of Purity", fileName = "Shards_of_Purity")]
public class ShardsOfPurity : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        finalValues.waterValues.power -= values.power;

        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Fire || card.element == CardElement.Electricity)
            {
                if (values.power > 0) values.power--;
            }
        }

        finalValues.waterValues.power += values.power;

        return finalValues;
    }
}
