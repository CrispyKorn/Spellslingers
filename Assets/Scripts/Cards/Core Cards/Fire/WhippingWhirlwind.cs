using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Whipping Whirlwind", fileName = "Whipping_Whirlwind")]
public class WhippingWhirlwind : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire) _finalValues.FireValues.Power += card.Values.Power;
            else
            {
                _finalValues.FireValues.Power = 0;
                break;
            }
        }

        return _finalValues;
    }
}
