using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Backdraft", fileName = "Backdraft")]
public class Backdraft : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        return _finalValues;
    }
}
