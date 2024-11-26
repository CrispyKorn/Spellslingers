using UnityEngine;
using System;

public abstract class UtilityCard : ScriptableObject, ICard
{
    public abstract event Action<UtilityCard, Deck> OnCardEffectComplete;

    public ICard.CardType Type { get => _type; }
    public string CardName { get => _cardName; }
    public string Description { get => _description; }
    public Sprite FrontImg { get => _frontImg; }
    public Sprite BackImg { get => _backImg; }

    [SerializeField] private ICard.CardType _type;
    [SerializeField] private string _cardName;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _frontImg;
    [SerializeField] private Sprite _backImg;

    public abstract void ApplyEffect(UtilityInfo utilityInfo);

    public void PrintDataToConsole()
    {
        Debug.Log($"{_cardName}: {_description}\nType: {_type}");
    }

    public int CompareTo(ICard otherCardObj)
    {
        return (int)Type - (int)otherCardObj.Type;
    }
}
