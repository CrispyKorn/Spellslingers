using UnityEngine;
using System;

public interface ICard : IComparable<ICard>
{
    public enum CardType
    {
        Core,
        Offence,
        Defence,
        Utility
    }

    public CardType Type { get; }
    public string CardName { get; }
    public string Description { get; }
    public Sprite FrontImg { get; }
    public Sprite BackImg { get; }

    public void PrintDataToConsole();

    /*
    public new int CompareTo(ICard otherCard)
    {
        if (Type == CardType.Utility || otherCard.Type == CardType.Utility) return (int)Type - (int)otherCard.Type;

        // Neither card is Utility
        Card cardA = this as Card;
        Card cardB = otherCard as Card;
        return cardA.CompareTo(cardB);
    }
    */
}
