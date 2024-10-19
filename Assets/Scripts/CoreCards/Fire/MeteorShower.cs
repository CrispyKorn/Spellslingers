using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Meteor Shower", fileName = "Meteor_Shower")]
public class MeteorShower : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        if (_peripheralCards.Length < 3) finalValues.fireValues.power -= values.power;

        return finalValues;
    }
}
