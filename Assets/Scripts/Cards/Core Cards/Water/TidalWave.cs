using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tidal Wave", fileName = "Tidal_Wave")]
public class TidalWave : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        CardValues combinedOffenceValues = _finalValues.OffenceValues.FireValues + _finalValues.OffenceValues.ElectricityValues;
        CardValues combinedDefenceValues = _finalValues.DefenceValues.FireValues + _finalValues.DefenceValues.ElectricityValues;

        _finalValues.OffenceValues.WaterValues += combinedOffenceValues;
        _finalValues.DefenceValues.WaterValues += combinedDefenceValues;

        _finalValues.OffenceValues.FireValues.Zero();
        _finalValues.OffenceValues.ElectricityValues.Zero();
        _finalValues.DefenceValues.FireValues.Zero();
        _finalValues.DefenceValues.ElectricityValues.Zero();
    }
}
