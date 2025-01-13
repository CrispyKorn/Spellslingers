

using Unity.Mathematics;

public class CombinedCardValues
{
    public CardValues WaterValues { get => _waterValues; set => _waterValues = value; }
    public CardValues FireValues { get => _fireValues; set => _fireValues = value; }
    public CardValues ElectricityValues { get => _electricityValues; set => _electricityValues = value; }

    private CardValues _waterValues;
    private CardValues _fireValues;
    private CardValues _electricityValues;

    public CombinedCardValues()
    {
        _waterValues = new();
        _fireValues = new();
        _electricityValues = new();
    }

    public CombinedCardValues(CardValues waterValues, CardValues fireValues, CardValues electricityValues)
    {
        _waterValues = waterValues;
        _fireValues = fireValues;
        _electricityValues = electricityValues;
    }

    public static CombinedCardValues operator +(CombinedCardValues left, CombinedCardValues right)
    {
        return new CombinedCardValues(left.WaterValues + right.WaterValues, left.FireValues + right.FireValues, left.ElectricityValues + right.ElectricityValues);
    }

    public static CombinedCardValues operator -(CombinedCardValues left, CombinedCardValues right)
    {
        return new CombinedCardValues(left.WaterValues - right.WaterValues, left.FireValues - right.FireValues, left.ElectricityValues - right.ElectricityValues);
    }
}
