using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Rainbow Rope", fileName = "Rainbow_Rope")]
public class RainbowRope : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        var usedWater = false;
        var usedElectricity = false;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water) usedWater = true;
            if (card.Element == CardElement.Electricity) usedElectricity = true;
        }

        var additionalSpellTypeNum = 0;
        if (usedWater) additionalSpellTypeNum++;
        if (usedElectricity) additionalSpellTypeNum++;

        _finalValues.OffenceValues.FireValues.Power += additionalSpellTypeNum;
    }
}
