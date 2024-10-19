using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public List<ICard> cards;

    public Deck()
    {
        cards = new List<ICard>();
    }

    public Deck(List<ICard> _cards)
    {
        cards = _cards;
    }

    public void AddCards(List<ICard> _cards)
    {
        foreach (ICard card in _cards)
        {
            cards.Add(card);
        }
    }

    public void AddCard(ICard card)
    {
        cards.Add(card);
    }

    public ICard DrawCard(int cardNum)
    {
        ICard card = cards[cardNum];
        cards.Remove(card);
        return card;
    }

    public void RemoveCard(ICard card)
    {
        if (cards.Contains(card)) cards.Remove(card);
    }
}
