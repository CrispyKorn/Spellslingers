using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CoreCard : Card
{
    // All calculations go through this card before being output to the game manager
    [System.Serializable]
    public enum CoreCardType
    {
        Normal,
        TurnExtender
    }

    public CoreCardType CoreType { get => _coreType; }

    [SerializeField] private CoreCardType _coreType;

    protected CombinedCardValues _finalValues;

    /// <summary>
    /// Calculates the total values of this core card and its peripheral cards for each element.
    /// </summary>
    /// <param name="peripheralCards">The peripheral cards associated with this core card.</param>
    /// <returns>The total combined values.</returns>
    public CombinedCardValues CalculateFinalValues(List<Card> peripheralCards)
    {
        _finalValues = new CombinedCardValues();

        // Add this card's values to the pool
        if (!IsNullfied)
        {
            switch (Element)
            {
                case CardElement.Electricity: _finalValues.ElectricityValues.Power += _values.Power; break;
                case CardElement.Fire: _finalValues.FireValues.Power += _values.Power; break;
                case CardElement.Water: _finalValues.WaterValues.Power += _values.Power; break;
            }
        }

        // Add all peripheral values to the pool
        for (int i = peripheralCards.Count - 1; i >= 0; i--)
        {
            if (peripheralCards[i].IsNullfied) peripheralCards.RemoveAt(i);
        }

        foreach (Card card in peripheralCards)
        {
            switch (card.Element)
            {
                case CardElement.Electricity:
                    {
                        _finalValues.ElectricityValues.Power += card.Values.Power;
                        _finalValues.ElectricityValues.Special += card.Values.Special;
                    }
                    break;
                case CardElement.Fire:
                    {
                        _finalValues.FireValues.Power += card.Values.Power;
                        _finalValues.FireValues.Special += card.Values.Special;
                    }
                    break;
                case CardElement.Water:
                    {
                        _finalValues.WaterValues.Power += card.Values.Power;
                        _finalValues.WaterValues.Special += card.Values.Special;
                    } 
                    break;
            }
        }

        if (!IsNullfied) ApplyEffect(peripheralCards.ToArray());
        return _finalValues;
    }

    /// <summary>
    /// Applies the effect of the core card it is called on.
    /// </summary>
    /// <param name="peripheralCards">The peripheral cards associated with this core card.</param>
    /// <returns>The combined values after applying the core card's effects.</returns>
    protected abstract CombinedCardValues ApplyEffect(Card[] peripheralCards);
}
