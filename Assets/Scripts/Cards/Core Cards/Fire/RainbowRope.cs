using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Rainbow Rope", fileName = "Rainbow_Rope")]
public class RainbowRope : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        var usedWater = false;
        var usedLight = false;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water) usedWater = true;
            if (card.Element == CardElement.Electricity) usedLight = true;
        }

        var additionalSpellTypeNum = 0;
        if (usedWater) additionalSpellTypeNum++;
        if (usedLight) additionalSpellTypeNum++;

        _finalValues.FireValues.Power += additionalSpellTypeNum;

        return _finalValues;
    }
}
