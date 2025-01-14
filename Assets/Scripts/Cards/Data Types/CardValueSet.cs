

public class CardValueSet
{
    public CombinedCardValues OffenceValues { get => _offenceValues; set => _offenceValues = value; }
    public CombinedCardValues DefenceValues { get => _defenceValues; set => _defenceValues = value; }

    private CombinedCardValues _offenceValues;
    private CombinedCardValues _defenceValues;

    public CardValueSet()
    {
        _offenceValues = new();
        _defenceValues = new();
    }

    public CardValueSet(CombinedCardValues offenceValues, CombinedCardValues defenceValues)
    {
        _offenceValues = offenceValues;
        _defenceValues = defenceValues;
    }

    public static CardValueSet operator +(CardValueSet left, CardValueSet right)
    {
        return new CardValueSet(left.OffenceValues + right.OffenceValues, left.DefenceValues + right.DefenceValues);
    }

    public static CardValueSet operator -(CardValueSet left, CardValueSet right)
    {
        return new CardValueSet(left.OffenceValues - right.OffenceValues, left.DefenceValues - right.DefenceValues);
    }
}
