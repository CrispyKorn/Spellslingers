using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Spewing Volcano", fileName = "Spewing_Volcano")]
public class SpewingVolcano : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        CardValues combinedOffenceValues = _finalValues.OffenceValues.WaterValues + _finalValues.OffenceValues.ElectricityValues;
        CardValues combinedDefenceValues = _finalValues.DefenceValues.WaterValues + _finalValues.DefenceValues.ElectricityValues;

        _finalValues.OffenceValues.FireValues += combinedOffenceValues;
        _finalValues.DefenceValues.FireValues += combinedDefenceValues;

        _finalValues.OffenceValues.WaterValues.Zero();
        _finalValues.OffenceValues.ElectricityValues.Zero();
        _finalValues.DefenceValues.WaterValues.Zero();
        _finalValues.DefenceValues.ElectricityValues.Zero();
    }
}
