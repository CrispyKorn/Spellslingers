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

    protected CardValueSet _finalValues;

    /// <summary>
    /// Calculates the total values of this core card and its peripheral cards for each element.
    /// </summary>
    /// <param name="peripheralCards">The peripheral cards associated with this core card.</param>
    /// <returns>The total combined values.</returns>
    public CardValueSet CalculateFinalValues(List<Card> peripheralCards)
    {
        _finalValues = new CardValueSet();

        // Add this card's values to the pool
        if (!IsNullfied)
        {
            switch (Element)
            {
                case CardElement.Electricity: _finalValues.OffenceValues.ElectricityValues += _values; break;
                case CardElement.Fire: _finalValues.OffenceValues.FireValues += _values; break;
                case CardElement.Water: _finalValues.OffenceValues.WaterValues += _values; break;
            }
        }

        // Remove all nullified peripheral cards
        for (int i = peripheralCards.Count - 1; i >= 0; i--)
        {
            if (peripheralCards[i].IsNullfied) peripheralCards.RemoveAt(i);
        }

        // Add all peripheral values to the pool
        foreach (Card card in peripheralCards)
        {
            CombinedCardValues relevantCombinedValues = card.Type == ICard.CardType.Offence ? _finalValues.OffenceValues : _finalValues.DefenceValues;

            switch (card.Element)
            {
                case CardElement.Electricity: relevantCombinedValues.ElectricityValues += card.Values; break;
                case CardElement.Fire: relevantCombinedValues.FireValues += card.Values; break;
                case CardElement.Water: relevantCombinedValues.WaterValues += card.Values; break;
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
    protected abstract void ApplyEffect(Card[] peripheralCards);
}
