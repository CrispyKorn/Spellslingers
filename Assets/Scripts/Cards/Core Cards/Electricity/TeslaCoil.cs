using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tesla Coil", fileName = "Tesla_Coil")]
public class TeslaCoil : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.OffenceValues.ElectricityValues -= _values;

        _values.Power -= peripheralCards.Length;
        if (_values.Power < 0) _values.Power = 0;

        _finalValues.OffenceValues.ElectricityValues += _values;
    }
}
