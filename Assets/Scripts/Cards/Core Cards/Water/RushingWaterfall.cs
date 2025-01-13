using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Rushing Waterfall", fileName = "Rushing_Waterfall")]
public class RushingWaterfall : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water) _finalValues.OffenceValues.WaterValues.Power++;
        }
    }
}
