using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Whispering Ember", fileName = "Whispering_Ember")]
public class WhisperingEmber : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.OffenceValues.FireValues -= _values;

        _values.Power -= peripheralCards.Length;
        if (_values.Power < 0) _values.Power = 0;

        _finalValues.OffenceValues.FireValues += _values;
    }
}
