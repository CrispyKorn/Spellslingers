using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Piercing Sunbeams", fileName = "Piercing_Sunbeams")]
public class PiercingSunbeams : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.OffenceValues.FireValues -= _values;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water || card.Element == CardElement.Electricity)
            {
                if (_values.Power > 0) _values.Power--;
            }
        }

        _finalValues.OffenceValues.FireValues += _values;
    }
}
