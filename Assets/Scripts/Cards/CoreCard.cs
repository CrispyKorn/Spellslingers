

public abstract class CoreCard : Card
{
    // All calculations go through this card before being output to the game manager

    protected CombinedCardValues _finalValues;

    public CombinedCardValues CalculateFinalValues(Card[] peripheralCards)
    {
        _finalValues = new CombinedCardValues();

        // Add this card's values to the pool
        switch (Element)
        {
            case CardElement.Electricity: _finalValues.ElectricityValues.Power += _values.Power; break;
            case CardElement.Fire: _finalValues.FireValues.Power += _values.Power; break;
            case CardElement.Water: _finalValues.WaterValues.Power += _values.Power; break;
        }

        // Add all peripheral values to the pool
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

        ApplyEffect(peripheralCards);
        return _finalValues;
    }

    protected abstract CombinedCardValues ApplyEffect(Card[] peripheralCards);
}
