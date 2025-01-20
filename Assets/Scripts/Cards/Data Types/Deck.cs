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

    /// <summary>
    /// Adds the give list of cards to the deck.
    /// </summary>
    /// <param name="cards">The cards to add to the deck.</param>
    public void AddCards(List<ICard> cards)
    {
        foreach (ICard card in cards)
        {
            _cards.Add(card);
        }
    }

    /// <summary>
    /// Adds a single card to the deck.
    /// </summary>
    /// <param name="card">The card to add to the deck.</param>
    public void AddCard(ICard card)
    {
        _cards.Add(card);
    }

    /// <summary>
    /// Draws a single card from the deck at random.
    /// </summary>
    /// <returns>The drawn card.</returns>
    public ICard DrawCard()
    {
        int cardToDrawIndex = UnityEngine.Random.Range(0, _cards.Count);
        ICard card = _cards[cardToDrawIndex];
        _cards.Remove(card);
        return card;
    }
    
    /// <summary>
    /// Removes the given card from the deck.
    /// </summary>
    /// <param name="card">The card to remove.</param>
    public void RemoveCard(ICard card)
    {
        if (_cards.Contains(card)) _cards.Remove(card);
    }

    /// <summary>
    /// Removes all cards from the deck.
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
    }
}
