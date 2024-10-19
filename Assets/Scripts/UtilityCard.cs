using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UtilityCard : ScriptableObject, ICard
{
    [SerializeField] ICard.CardType type;
    [SerializeField] string cardName;
    [SerializeField] string description;
    [SerializeField] Sprite frontImg;
    [SerializeField] Sprite backImg;

    public abstract event System.Action<UtilityCard, Deck> OnCardEffectComplete;

    public ICard.CardType Type { get { return type; } set { type = value; } }
    public string CardName { get { return cardName; } set { cardName = value; } }
    public string Description { get { return description; } set { description = value; } }
    public Sprite FrontImg { get { return frontImg; } set { frontImg = value; } }
    public Sprite BackImg { get { return backImg; } set { backImg = value; } }

    public abstract void ApplyEffect(UtilityManager.UtilityInfo utilityInfo);

    public void PrintDataToConsole()
    {
        Debug.Log(CardName + ": " + Description + "\nType: " + Type);
    }

    public int CompareTo(ICard otherCardObj)
    {
        return (int)Type - (int)otherCardObj.Type;
    }
}
