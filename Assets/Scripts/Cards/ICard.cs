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

    /// <summary>
    /// Prints the card's main data to the console. Used for debug purposes only.
    /// </summary>
    public void PrintDataToConsole();

    public new int CompareTo(ICard otherCardObj);
}
