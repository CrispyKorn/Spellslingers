using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Gurgling Cauldron", fileName = "Gurgling_Cauldron")]
public class GurglingCauldron : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        var usedFire = false;
        var usedElectricity = false;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire) usedFire = true;
            if (card.Element == CardElement.Electricity) usedElectricity = true;
        }

        var additionalSpellTypeNum = 0;
        if (usedFire) additionalSpellTypeNum++;
        if (usedElectricity) additionalSpellTypeNum++;

        _finalValues.OffenceValues.FireValues.Power += additionalSpellTypeNum;
    }
}
