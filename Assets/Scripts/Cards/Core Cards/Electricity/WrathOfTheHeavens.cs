using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Wrath of the Heavens", fileName = "Wrath_Of_The_Heavens")]
public class WrathOfTheHeavens : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        if (peripheralCards.Length < 3) _finalValues.OffenceValues.ElectricityValues -= _values;
    }
}
