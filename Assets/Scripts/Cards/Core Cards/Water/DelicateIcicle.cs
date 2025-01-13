using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Delicate Icicle", fileName = "Delicate_Icicle")]
public class DelicateIcicle : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.OffenceValues.WaterValues -= _values;

        _values.Power -= peripheralCards.Length;
        if (_values.Power < 0) _values.Power = 0;

        _finalValues.OffenceValues.WaterValues += _values;
    }
}
