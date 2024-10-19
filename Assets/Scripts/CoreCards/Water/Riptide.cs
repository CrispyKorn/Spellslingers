using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Riptide", fileName = "Riptide")]
public class Riptide : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] _peripheralCards)
    {
        return finalValues;
    }
}
