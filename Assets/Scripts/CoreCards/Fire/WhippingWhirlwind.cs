using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Whipping Whirlwind", fileName = "Whipping_Whirlwind")]
public class WhippingWhirlwind : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Fire) finalValues.fireValues.power += card.values.power;
            else
            {
                finalValues.fireValues.power = 0;
                break;
            }
        }

        return finalValues;
    }
}
