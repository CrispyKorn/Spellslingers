using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Riptide", fileName = "Riptide")]
public class Riptide : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        return _finalValues;
    }
}
