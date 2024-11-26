using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject, ICard
{    
    public enum CardElement
    {
        Water,
        Fire,
        Electricity
    }
    
    public ICard.CardType Type { get => _type; }
    public CardElement Element { get => _element; }
    public CardValues Values { get => _values; }
    public string CardName { get => _cardName; }
    public string Description { get => _description; }
    public Sprite FrontImg { get => _frontImg; }
    public Sprite BackImg { get => _backImg; }

    [SerializeField] private ICard.CardType _type;
    [SerializeField] private string _cardName;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _frontImg;
    [SerializeField] private Sprite _backImg;

    protected CardElement _element;
    protected CardValues _values;

    public void PrintDataToConsole()
    {
        Debug.Log($"{_cardName}: {_description}\nType: {_type} | Element: {_element} | Power: {_values.Power} | Special: {_values.Special}");
    }

    public int CompareTo(ICard otherCardObj)
    {
        if (Type == ICard.CardType.Utility || otherCardObj.Type == ICard.CardType.Utility) return (int)Type - (int)otherCardObj.Type;

        var otherCard = otherCardObj as Card;

        // Calculate card values with priority ordering of: Type -> Element -> Power -> Special
        int aTypeValue = ((int)Type + 1) * 1000;
        int bTypeValue = ((int)otherCard.Type + 1) * 1000;
        int aElementValue = ((int)_element + 1) * 100;
        int bElementValue = ((int)otherCard._element + 1) * 100;
        int aPowerValue = 90 - (_values.Power + 1) * 10;
        int bPowerValue = 90 - (otherCard._values.Power + 1) * 10;
        int aSpecialValue = 9 - _values.Special;
        int bSpecialValue = 9 - otherCard._values.Special;

        int aValue = aTypeValue + aElementValue + aPowerValue + aSpecialValue;
        int bValue = bTypeValue + bElementValue + bPowerValue + bSpecialValue;

        return aValue.CompareTo(bValue);
    }
}
