using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tidal Wave", fileName = "Tidal_Wave")]
public class TidalWave : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        CardValues combinedValues = new();
        combinedValues.Power = _finalValues.FireValues.Power + _finalValues.ElectricityValues.Power;
        combinedValues.Special = _finalValues.FireValues.Special + _finalValues.ElectricityValues.Special;

        _finalValues.WaterValues.Power += combinedValues.Power;
        _finalValues.WaterValues.Special += combinedValues.Special;

        _finalValues.FireValues.Power = 0;
        _finalValues.FireValues.Special = 0;
        _finalValues.ElectricityValues.Power = 0;
        _finalValues.ElectricityValues.Special = 0;

        return _finalValues;
    }
}
