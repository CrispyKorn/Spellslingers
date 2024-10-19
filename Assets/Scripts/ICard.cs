using System.Collections;
using System.Collections.Generic;
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

    CardType Type { get; set; }
    string CardName { get; set; }
    string Description { get; set; }
    Sprite FrontImg { get; set; }
    Sprite BackImg { get; set; }

    public void PrintDataToConsole();

    /*
    public new int CompareTo(ICard otherCard)
    {
        if (Type == CardType.Utility || otherCard.Type == CardType.Utility) return (int)Type - (int)otherCard.Type;

        //Neither card is Utility
        Card cardA = this as Card;
        Card cardB = otherCard as Card;
        return cardA.CompareTo(cardB);
    }
    */
}
