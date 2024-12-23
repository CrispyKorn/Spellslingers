

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
        _waterValues = new CardValues();
        _fireValues = new CardValues();
        _electricityValues = new CardValues();
    }
}
