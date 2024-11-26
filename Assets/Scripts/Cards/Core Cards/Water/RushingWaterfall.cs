using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Rushing Waterfall", fileName = "Rushing_Waterfall")]
public class RushingWaterfall : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water) _finalValues.WaterValues.Power++;
        }

        return _finalValues;
    }
}
