using System.Collections.Generic;
using UnityEngine;

public class Hand
{
    public GameObject[] CardObjs { get => _cardObjs.ToArray(); }
    public int Size { get => _cardObjs.Count; }

    private Deck _deck = new();
    private List<GameObject> _cardObjs = new();
    private Dictionary<GameObject, ICard> _objToCard = new();

    /// <summary>
    /// Adds a single card to the hand.
    /// </summary>
    /// <param name="card">The card to add to the hand.</param>
    /// <param name="cardObj">The GameObject representing the card.</param>
    public void AddCard(ICard card, GameObject cardObj)
    {
        _deck.AddCard(card);
        _cardObjs.Add(cardObj);
        _objToCard.Add(cardObj, card);
    }

    /// <summary>
    /// Adds a set of cards to the hand.
    /// </summary>
    /// <param name="cards">The cards to add to the hand.</param>
    /// <param name="cardObjs">The GameObjects representing the cards. Should match the ordering of the cards list.</param>
    public void AddCards(List<ICard> cards, List<GameObject> cardObjs)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            AddCard(cards[i], cardObjs[i]);
        }
    }

    /// <summary>
    /// Removes a card from the hand.
    /// </summary>
    /// <param name="cardObj">The GameObject representing the card to remove.</param>
    public void RemoveCard(GameObject cardObj)
    {
        ICard card = GetCardFromObj(cardObj);
        _deck.RemoveCard(card);
        _cardObjs.Remove(cardObj);
        _objToCard.Remove(cardObj);
    }

    /// <summary>
    /// Removes all cards from the hand.
    /// </summary>
    public void Clear()
    {
        _deck.Clear();
        _cardObjs.Clear();
        _objToCard.Clear();
    }

    /// <summary>
    /// Sorts the given cards.
    /// </summary>
    public void SortHand()
    {
        _cardObjs.Sort((a, b) =>
        {
            ICard cardA = a.GetComponent<PlayCard>().CardData;
            ICard cardB = b.GetComponent<PlayCard>().CardData;

            return cardA.CompareTo(cardB);
        });
    }

    public ICard GetCardFromObj(GameObject cardObj)
    {
        return _objToCard[cardObj];
    }
}
