using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Piercing Sunbeams", fileName = "Piercing_Sunbeams")]
public class PiercingSunbeams : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.FireValues.Power -= _values.Power;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water || card.Element == CardElement.Electricity)
            {
                if (_values.Power > 0) _values.Power--;
            }
        }

        _finalValues.FireValues.Power += _values.Power;

        return _finalValues;
    }
}
