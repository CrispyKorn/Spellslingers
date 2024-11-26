using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Wrath of the Heavens", fileName = "Wrath_Of_The_Heavens")]
public class WrathOfTheHeavens : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        if (peripheralCards.Length < 3) _finalValues.ElectricityValues.Power -= _values.Power;

        return _finalValues;
    }
}
