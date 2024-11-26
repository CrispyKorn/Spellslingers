using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Delicate Icicle", fileName = "Delicate_Icicle")]
public class DelicateIcicle : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.WaterValues.Power -= _values.Power;

        foreach (Card card in peripheralCards)
        {
            if (_values.Power > 0) _values.Power--;
        }

        _finalValues.WaterValues.Power += _values.Power;

        return _finalValues;
    }
}
