using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Radiant Lightshow", fileName = "Radiant_Lightshow")]
public class RadiantLightshow : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        var usedFire = false;
        var usedWater = false;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire) usedFire = true;
            if (card.Element == CardElement.Water) usedWater = true;
        }

        var additionalSpellTypeNum = 0;
        if (usedFire) additionalSpellTypeNum++;
        if (usedWater) additionalSpellTypeNum++;

        _finalValues.OffenceValues.ElectricityValues.Power += additionalSpellTypeNum;
    }
}
