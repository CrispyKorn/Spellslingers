using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Pillar of Flame", fileName = "Pillar_Of_Flame")]
public class PillarOfFlame : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Fire) finalValues.fireValues.power++;
        }

        return finalValues;
    }
}
