using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Meteor Shower", fileName = "Meteor_Shower")]
public class MeteorShower : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        if (peripheralCards.Length < 3) _finalValues.OffenceValues.FireValues -= _values;
    }
}
