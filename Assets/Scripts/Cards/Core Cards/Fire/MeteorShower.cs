using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Meteor Shower", fileName = "Meteor_Shower")]
public class MeteorShower : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        if (peripheralCards.Length < 3) _finalValues.FireValues.Power -= _values.Power;

        return _finalValues;
    }
}
