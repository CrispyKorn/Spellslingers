using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Raging Typhoon", fileName = "Raging_Typhoon")]
public class RagingTyphoon : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Water) finalValues.waterValues.power += card.values.power;
            else
            {
                finalValues.waterValues.power = 0;
                break;
            }
        }

        return finalValues;
    }
}
