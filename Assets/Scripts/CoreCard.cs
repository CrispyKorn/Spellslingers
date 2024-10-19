using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CoreCard : Card
{
    //All calculations go through this card before being output to the gameManager
    public struct CombinedCardValues
    {
        public CardValues waterValues;
        public CardValues fireValues;
        public CardValues electricityValues;
    }

    protected CombinedCardValues finalValues;

    public CombinedCardValues CalculateFinalValues(Card[] _peripheralCards)
    {
        finalValues = new CombinedCardValues();

        switch (element)
        {
            case CardElement.Electricity: finalValues.electricityValues.power += values.power; break;
            case CardElement.Fire: finalValues.fireValues.power += values.power; break;
            case CardElement.Water: finalValues.waterValues.power += values.power; break;
        }

        foreach (Card card in _peripheralCards)
        {
            switch (card.element)
            {
                case CardElement.Electricity:
                    {
                        finalValues.electricityValues.power += card.values.power;
                        finalValues.electricityValues.special += card.values.special;
                    }
                    break;
                case CardElement.Fire:
                    {
                        finalValues.fireValues.power += card.values.power;
                        finalValues.fireValues.special += card.values.special;
                    }
                    break;
                case CardElement.Water:
                    {
                        finalValues.waterValues.power += card.values.power;
                        finalValues.waterValues.special += card.values.special;
                    } 
                    break;
            }
        }

        ApplyEffect(_peripheralCards);
        return finalValues;
    }

    protected abstract CombinedCardValues ApplyEffect(Card[] _peripheralCards);
}
