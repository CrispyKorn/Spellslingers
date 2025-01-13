using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Pillar of Flame", fileName = "Pillar_Of_Flame")]
public class PillarOfFlame : CoreCard
{
    protected override void ApplyEffect(Card[] peripheralCards)
    {
        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire) _finalValues.OffenceValues.FireValues.Power++;
        }
    }
}
