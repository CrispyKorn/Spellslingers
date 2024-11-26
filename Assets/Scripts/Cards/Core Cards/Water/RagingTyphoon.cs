using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Raging Typhoon", fileName = "Raging_Typhoon")]
public class RagingTyphoon : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water) _finalValues.WaterValues.Power += card.Values.Power;
            else
            {
                _finalValues.WaterValues.Power = 0;
                break;
            }
        }

        return _finalValues;
    }
}
