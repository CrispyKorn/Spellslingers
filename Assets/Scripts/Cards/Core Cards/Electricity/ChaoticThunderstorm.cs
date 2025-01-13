using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Chaotic Thunderstorm", fileName = "Chaotic_Thunderstorm")]
public class ChaoticThunderstorm : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Electricity) 
            {
                CombinedCardValues relevantCombinedValues = card.Type == ICard.CardType.Offence ? _finalValues.OffenceValues : _finalValues.DefenceValues;
                relevantCombinedValues.ElectricityValues.Power += card.Values.Power;
            }
            else
            {
                _finalValues.OffenceValues.ElectricityValues.Zero();
                _finalValues.DefenceValues.ElectricityValues.Zero();

                break;
            }
        }
    }
}
