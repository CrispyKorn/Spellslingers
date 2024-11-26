using UnityEngine;

[CreateAssetMenu(menuName = "Core Card/Shards of Purity", fileName = "Shards_of_Purity")]
public class ShardsOfPurity : CoreCard
{
    protected override CombinedCardValues ApplyEffect(Card[] peripheralCards)
    {
        _finalValues.WaterValues.Power -= _values.Power;

        foreach (Card card in peripheralCards)
        {
            if (card.Element == CardElement.Fire || card.Element == CardElement.Electricity)
            {
                if (_values.Power > 0) _values.Power--;
            }
        }

        _finalValues.WaterValues.Power += _values.Power;

        return _finalValues;
    }
}
