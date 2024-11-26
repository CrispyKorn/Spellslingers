using System.Collections.Generic;

public class Deck
{
    public List<ICard> Cards { get => _cards; }

    private List<ICard> _cards = new();

    public Deck()
    {
        
    }

    public Deck(List<ICard> cards)
    {
        _cards = cards;
    }

    public void AddCards(List<ICard> cards)
    {
        foreach (ICard card in cards)
        {
            _cards.Add(card);
        }
    }

    public void AddCard(ICard card)
    {
        _cards.Add(card);
    }

    public ICard DrawCard(int cardNum)
    {
        ICard card = _cards[cardNum];
        _cards.Remove(card);
        return card;
    }

    public void RemoveCard(ICard card)
    {
        if (_cards.Contains(card)) _cards.Remove(card);
    }
}
