using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Flickering Lightningbugs", fileName = "Flickering_Lightningbugs")]
public class FlickeringLightningBugs : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.OffenceValues.ElectricityValues -= _values;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire || card.Element == CardElement.Water)
            {
                if (_values.Power > 0) _values.Power--;
            }
        }

        _finalValues.OffenceValues.ElectricityValues += _values;
    }
}
