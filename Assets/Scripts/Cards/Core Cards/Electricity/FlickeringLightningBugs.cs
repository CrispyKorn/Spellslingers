using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Flickering Lightningbugs", fileName = "Flickering_Lightningbugs")]
public class FlickeringLightningBugs : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.ElectricityValues.Power -= _values.Power;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire || card.Element == CardElement.Water)
            {
                if (_values.Power > 0) _values.Power--;
            }
        }

        _finalValues.ElectricityValues.Power += _values.Power;

        return _finalValues;
    }
}
