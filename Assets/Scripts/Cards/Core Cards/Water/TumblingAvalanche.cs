using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Tumbling Avalanche", fileName = "Tumbling_Avalanche")]
public class TumblingAvalanche : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        if (peripheralCards.Length < 3) _finalValues.OffenceValues.WaterValues.Power -= _values.Power;
    }
}
