using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tumbling Avalanche", fileName = "Tumbling_Avalanche")]
public class TumblingAvalanche : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        if (peripheralCards.Length < 3) _finalValues.WaterValues.Power -= _values.Power;

        return _finalValues;
    }
}
