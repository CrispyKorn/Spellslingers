using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class Card : ScriptableObject, ICard
{    
    public enum CardElement
    {
        Water,
        Fire,
        Electricity
    }

    [System.Serializable]
    public struct CardValues
    {
        public int power;
        public int special;
    }

    [SerializeField] ICard.CardType type;
    [SerializeField] string cardName;
    [SerializeField] string description;
    [SerializeField] Sprite frontImg;
    [SerializeField] Sprite backImg;

    public CardElement element;
    public CardValues values;

    public ICard.CardType Type { get { return type; } set { type = value; } }
    public string CardName { get { return cardName; } set { cardName = value; } }
    public string Description { get { return description; } set { description = value; } }
    public Sprite FrontImg { get { return frontImg; } set { frontImg = value; } }
    public Sprite BackImg{ get { return backImg; } set { backImg = value; } }

    public void PrintDataToConsole()
    {
        Debug.Log(CardName + ": " + Description + "\nType: " + Type + " | Element: " + element + " | Power: " + values.power + " | Special: " + values.special);
    }

    public int CompareTo(ICard otherCardObj)
    {
        if (Type == ICard.CardType.Utility || otherCardObj.Type == ICard.CardType.Utility) return (int)Type - (int)otherCardObj.Type;

        Card otherCard = otherCardObj as Card;

        int aTypeValue = ((int)Type + 1) * 1000;
        int bTypeValue = ((int)otherCard.Type + 1) * 1000;
        int aElementValue = ((int)element + 1) * 100;
        int bElementValue = ((int)otherCard.element + 1) * 100;
        int aPowerValue = 90 - (values.power + 1) * 10;
        int bPowerValue = 90 - (otherCard.values.power + 1) * 10;
        int aSpecialValue = 9 - values.special;
        int bSpecialValue = 9 - otherCard.values.special;

        int aValue = aTypeValue + aElementValue + aPowerValue + aSpecialValue;
        int bValue = bTypeValue + bElementValue + bPowerValue + bSpecialValue;

        return aValue.CompareTo(bValue);
    }
}
