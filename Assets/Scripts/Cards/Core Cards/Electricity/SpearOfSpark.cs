using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Spear of Spark", fileName = "Spear_Of_Spark")]
public class SpearOfSpark : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Electricity) _finalValues.ElectricityValues.Power++;
        }

        return _finalValues;
    }
}
