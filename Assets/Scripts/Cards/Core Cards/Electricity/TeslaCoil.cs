using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tesla Coil", fileName = "Tesla_Coil")]
public class TeslaCoil : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.ElectricityValues.Power -= _values.Power;

        foreach (Card card in peripheralCards)
        {
            if (_values.Power > 0) _values.Power--;
        }

        _finalValues.ElectricityValues.Power += _values.Power;

        return _finalValues;
    }
}
