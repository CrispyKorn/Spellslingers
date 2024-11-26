using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Vortex of Lightning", fileName = "Vortex_Of_Lightning")]
public class VortexOfLightning : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        CardValues combinedValues = new();
        combinedValues.Power = _finalValues.FireValues.Power + _finalValues.WaterValues.Power;
        combinedValues.Special = _finalValues.FireValues.Special + _finalValues.WaterValues.Special;

        _finalValues.ElectricityValues.Power += combinedValues.Power;
        _finalValues.ElectricityValues.Special += combinedValues.Special;

        _finalValues.FireValues.Power = 0;
        _finalValues.FireValues.Special = 0;
        _finalValues.WaterValues.Power = 0;
        _finalValues.WaterValues.Special = 0;

        return _finalValues;
    }
}
