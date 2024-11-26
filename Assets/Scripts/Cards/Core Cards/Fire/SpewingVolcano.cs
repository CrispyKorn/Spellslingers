using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Spewing Volcano", fileName = "Spewing_Volcano")]
public class SpewingVolcano : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        CardValues combinedValues = new();
        combinedValues.Power = _finalValues.WaterValues.Power + _finalValues.ElectricityValues.Power;
        combinedValues.Special = _finalValues.WaterValues.Special + _finalValues.ElectricityValues.Special;

        _finalValues.FireValues.Power += combinedValues.Power;
        _finalValues.FireValues.Special += combinedValues.Special;

        _finalValues.WaterValues.Power = 0;
        _finalValues.WaterValues.Special = 0;
        _finalValues.ElectricityValues.Power = 0;
        _finalValues.ElectricityValues.Special = 0;

        return _finalValues;
    }
}
