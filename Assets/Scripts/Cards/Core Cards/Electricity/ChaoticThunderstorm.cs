using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Chaotic Thunderstorm", fileName = "Chaotic_Thunderstorm")]
public class ChaoticThunderstorm : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Electricity) _finalValues.ElectricityValues.Power += card.Values.Power;
            else
            {
                _finalValues.ElectricityValues.Power = 0;
                break;
            }
        }

        return _finalValues;
    }
}
