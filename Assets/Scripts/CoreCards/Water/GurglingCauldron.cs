using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Gurgling Cauldron", fileName = "Gurgling_Cauldron")]
public class GurglingCauldron : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        bool usedFire = false;
        bool usedLight = false;

        foreach (Card card in _peripheralCards)
        {
            if (card.element == CardElement.Fire) usedFire = true;
            if (card.element == CardElement.Electricity) usedLight = true;
        }

        int additionalSpellTypeNum = 0;
        if (usedFire) additionalSpellTypeNum++;
        if (usedLight) additionalSpellTypeNum++;

        finalValues.waterValues.power += additionalSpellTypeNum;

        return finalValues;
    }
}
