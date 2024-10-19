using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Rainbow Rope", fileName = "Rainbow_Rope")]
public class RainbowRope : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        bool usedWater = false;
        bool usedLight = false;

        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Water) usedWater = true;
            if (card.element == CardElement.Electricity) usedLight = true;
        }

        int additionalSpellTypeNum = 0;
        if (usedWater) additionalSpellTypeNum++;
        if (usedLight) additionalSpellTypeNum++;

        finalValues.fireValues.power += additionalSpellTypeNum;

        return finalValues;
    }
}
