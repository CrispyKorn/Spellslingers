using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Gurgling Cauldron", fileName = "Gurgling_Cauldron")]
public class GurglingCauldron : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        var usedFire = false;
        var usedLight = false;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire) usedFire = true;
            if (card.Element == CardElement.Electricity) usedLight = true;
        }

        var additionalSpellTypeNum = 0;
        if (usedFire) additionalSpellTypeNum++;
        if (usedLight) additionalSpellTypeNum++;

        _finalValues.WaterValues.Power += additionalSpellTypeNum;

        return _finalValues;
    }
}
