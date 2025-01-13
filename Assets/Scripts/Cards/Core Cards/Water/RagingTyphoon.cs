using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Raging Typhoon", fileName = "Raging_Typhoon")]
public class RagingTyphoon : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Water) 
            {
                CombinedCardValues relevantCombinedValues = card.Type == ICard.CardType.Offence ? _finalValues.OffenceValues : _finalValues.DefenceValues;
                relevantCombinedValues.WaterValues.Power += card.Values.Power;
            }
            else
            {
                _finalValues.OffenceValues.WaterValues.Zero();
                _finalValues.DefenceValues.WaterValues.Zero();

                break;
            }
        }
    }
}
