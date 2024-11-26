using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Whispering Ember", fileName = "Whispering_Ember")]
public class WhisperingEmber : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.FireValues.Power -= _values.Power;

        foreach (Card card in peripheralCards)
        {
            if (_values.Power > 0) _values.Power--;
        }

        _finalValues.FireValues.Power += _values.Power;

        return _finalValues;
    }
}
