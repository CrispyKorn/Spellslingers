using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Whipping Whirlwind", fileName = "Whipping_Whirlwind")]
public class WhippingWhirlwind : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire) 
            {
                CombinedCardValues relevantCombinedValues = card.Type == ICard.CardType.Offence ? _finalValues.OffenceValues : _finalValues.DefenceValues;
                relevantCombinedValues.FireValues.Power += card.Values.Power;
            }
            else
            {
                _finalValues.OffenceValues.FireValues.Zero();
                _finalValues.DefenceValues.FireValues.Zero();

                break;
            }
        }
    }
}
