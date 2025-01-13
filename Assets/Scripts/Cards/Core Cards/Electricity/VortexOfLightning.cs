using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Vortex of Lightning", fileName = "Vortex_Of_Lightning")]
public class VortexOfLightning : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        CardValues combinedOffenceValues = _finalValues.OffenceValues.FireValues + _finalValues.OffenceValues.WaterValues;
        CardValues combinedDefenceValues = _finalValues.DefenceValues.FireValues + _finalValues.DefenceValues.WaterValues;

        _finalValues.OffenceValues.ElectricityValues += combinedOffenceValues;
        _finalValues.DefenceValues.ElectricityValues += combinedDefenceValues;

        _finalValues.OffenceValues.FireValues.Zero();
        _finalValues.OffenceValues.WaterValues.Zero();
        _finalValues.DefenceValues.FireValues.Zero();
        _finalValues.DefenceValues.WaterValues.Zero();
    }
}
