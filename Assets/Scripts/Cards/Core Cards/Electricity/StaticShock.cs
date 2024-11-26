using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Static Shock", fileName = "Static_Shock")]
public class StaticShock : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        return _finalValues;
    }
}
