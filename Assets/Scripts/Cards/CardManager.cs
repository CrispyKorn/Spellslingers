using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using System;

public class CardManager : NetworkBehaviour
{
    [Serializable]
    private struct CardData<ICard>
    {
        public ICard Card { get => _card; }
        public int Amount { get => _amount; }

        [SerializeField] private ICard _card;
        [SerializeField] private int _amount;

        public CardData(ICard card, int amount)
        {
            _card = card;
            _amount = amount;
        }
    }

    public NetworkList<int> N_Player1CardsIndices { get => n_player1CardsIndices; }
    public NetworkList<int> N_Player2CardsIndices { get => n_player2CardsIndices; }
    public List<GameObject> Player1Cards { get => _player1Cards; }
    public List<GameObject> Player2Cards { get => _player2Cards; }
    public Dictionary<int, ICard> CardIndexToCard { get => _cardIndexToCard; }
    public Dictionary<ICard, int> CardToCardIndex { get => _cardToCardIndex; }
    public Deck OffenceDeck { get => _offenceDeck; }
    public Deck DefenceDeck { get => _defenceDeck; }
    public Deck CoreDeck { get => _coreDeck; }
    public Deck UtilityDeck { get => _utilityDeck; }

    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<CardData<Card>> _allNormalCards = new();
    [SerializeField] private List<CardData<UtilityCard>> _allUtilityCards = new();

    private NetworkList<int> n_player1CardsIndices;
    private NetworkList<int> n_player2CardsIndices;

    private List<GameObject> _player1Cards = new();
    private List<GameObject> _player2Cards = new();
    private Dictionary<int, ICard> _cardIndexToCard = new();
    private Dictionary<ICard, int> _cardToCardIndex = new();
    private Deck _offenceDeck = new();
    private Deck _defenceDeck = new();
    private Deck _coreDeck = new();
    private Deck _utilityDeck = new();
    private List<CardData<ICard>> _allCards = new();

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);

        n_player1CardsIndices = new();
        n_player2CardsIndices = new();
    }

    /// <summary>
    /// Populates various lists for card management.
    /// </summary>
    private void InitializeCardSets()
    {
        foreach (CardData<Card> cardData in _allNormalCards)
        {
            var newCard = new CardData<ICard>(cardData.Card, cardData.Amount);
            _allCards.Add(newCard);
        }

        foreach (CardData<UtilityCard> cardAmount in _allUtilityCards)
        {
            var newCard = new CardData<ICard>(cardAmount.Card, cardAmount.Amount);
            _allCards.Add(newCard);
        }

        for (var i = 0; i < _allCards.Count; i++)
        {
            CardIndexToCard.Add(i, _allCards[i].Card);
            CardToCardIndex.Add(_allCards[i].Card, i);
        }
    }

    /// <summary>
    /// Sets up a players hand of cards with proper orientation, ownership and parent.
    /// Note: This runs over a number of frames to keep the network from being overloaded with calls.
    /// </summary>
    /// <param name="isPlayer1Cards">Whether to update the hand of player 1 (host).</param>
    /// <param name="player2ClientId">Client ID of player 2.</param>
    /// <param name="player1">Transform of the player 1 gameobject.</param>
    /// <param name="player2">Transform of the player 2 gameobject.</param>
    private async Task UpdateHand(bool isPlayer1Cards, ulong player2ClientId, Transform player1, Transform player2)
    {
        List<GameObject> playerCards = isPlayer1Cards ? _player1Cards : _player2Cards;

        foreach (GameObject cardObj in playerCards)
        {
            var playCard = cardObj.GetComponent<PlayCard>();

            // Set card ownership based on player (host is same as server)
            bool cardOwnedByServer = playCard.NetworkObject.IsOwnedByServer;
            if (isPlayer1Cards && !cardOwnedByServer) playCard.NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            if (!isPlayer1Cards && cardOwnedByServer) playCard.NetworkObject.ChangeOwnership(player2ClientId);

            cardObj.transform.SetParent(isPlayer1Cards ? player1 : player2);

            await Awaitable.NextFrameAsync();
        }

        // Sort Hands
        SortPlayerCards(playerCards);
        await SpreadCards(playerCards, isPlayer1Cards);
    }

    /// <summary>
    /// Draws a set of cards into a players hand, updating their tracking list.
    /// </summary>
    /// <param name="hand">The hand to draw the cards into.</param>
    /// <param name="playerCardsIndices">The list of card indices for the drawing player's hand.</param>
    private void InitializePlayerHand(Deck hand, NetworkList<int> playerCardsIndices)
    {
        hand.AddCards(Draw(CoreDeck, 3));
        hand.AddCards(Draw(OffenceDeck, 6));
        hand.AddCards(Draw(DefenceDeck, 6));
        hand.AddCards(Draw(UtilityDeck, 3));

        List<int> cardIndices = GetCardIndices(hand.Cards);
        foreach (int cardIndex in cardIndices) playerCardsIndices.Add(cardIndex);
    }

    public override void OnNetworkSpawn()
    {
        InitializeCardSets();
        PopulateDecks();
    }

    /// <summary>
    /// Populates each deck with the relevant cards.
    /// </summary>
    public void PopulateDecks()
    {
        foreach (CardData<ICard> cardData in _allCards)
        {
            switch (cardData.Card.Type)
            {
                case ICard.CardType.Offence:
                    {
                        for (var i = cardData.Amount; i > 0; i--) _offenceDeck.AddCard((Card)cardData.Card);
                    }
                    break;
                case ICard.CardType.Defence:
                    {
                        for (var i = cardData.Amount; i > 0; i--) _defenceDeck.AddCard((Card)cardData.Card);
                    }
                    break;
                case ICard.CardType.Core:
                    {
                        for (var i = cardData.Amount; i > 0; i--) _coreDeck.AddCard((Card)cardData.Card);
                    }
                    break;
                case ICard.CardType.Utility:
                    {
                        if (cardData.Card.CardName != "Mental Block") break;
                        for (var i = cardData.Amount; i > 0; i--) _utilityDeck.AddCard((UtilityCard)cardData.Card);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Initializes both players' hands with live cards.
    /// </summary>
    /// <param name="player1">Player 1 object (host).</param>
    /// <param name="player2">Player 2 object (client).</param>
    /// <param name="player2ClientId">Player 2's client ID.</param>
    public async void InitializePlayerCards(Player player1, Player player2, ulong player2ClientId)
    {
        InitializePlayerHand(player1.Hand, n_player1CardsIndices);
        InitializePlayerHand(player2.Hand, n_player2CardsIndices);

        await InstantiateCards(player1.Hand.Cards, true, player2ClientId, player1, player2);
        await InstantiateCards(player2.Hand.Cards, false, player2ClientId, player1, player2);
    }

    /// <summary>
    /// Adds the relevant cards to each players hand for the end of a round.
    /// </summary>
    /// <param name="player1">Player 1 object (host).</param>
    /// <param name="player2">Player 2 object (client).</param>
    /// <param name="player2ClientId">Player 2's client ID.</param>
    public async Task AddEndOfRoundCards(Player player1, Player player2, ulong player2ClientId)
    {
        List<ICard> player1Set = DrawEndOfRoundSet();
        List<ICard> player2Set = DrawEndOfRoundSet();

        player1.Hand.AddCards(player1Set);
        player2.Hand.AddCards(player2Set);

        await InstantiateCards(player1Set, true, player2ClientId, player1, player2);
        await InstantiateCards(player2Set, false, player2ClientId, player1, player2);
    }

    /// <summary>
    /// Draws the required cards for the end of a round.
    /// </summary>
    /// <returns>The set of cards drawn.</returns>
    private List<ICard> DrawEndOfRoundSet()
    {
        var newCards = new List<ICard>();
        ICard offenceCard = DrawOne(OffenceDeck);
        ICard defenceCard = DrawOne(DefenceDeck);
        ICard coreCard = DrawOne(CoreDeck);
        ICard utilityCard = DrawOne(UtilityDeck);

        if (offenceCard != null) newCards.Add(offenceCard);
        if (defenceCard != null) newCards.Add(defenceCard);
        if (coreCard != null) newCards.Add(coreCard);
        if (utilityCard != null) newCards.Add(utilityCard);

        return newCards;
    }

    /// <summary>
    /// Randomly draws the given number of cards from the given deck.
    /// </summary>
    /// <param name="drawDeck">The deck to draw from.</param>
    /// <param name="numOfCards">The number of cards to draw.</param>
    /// <returns>The set of cards drawn.</returns>
    public List<ICard> Draw(Deck drawDeck, int numOfCards)
    {
        List<ICard> cards = new();

        for (var i = 0; i < numOfCards; i++)
        {
            if (drawDeck.Cards.Count == 0) break;

            cards.Add(drawDeck.DrawCard());
        }

        return cards;
    }

    /// <summary>
    /// Draws one card from the given deck at random.
    /// </summary>
    /// <param name="drawDeck">The deck to draw from.</param>
    /// <returns>The drawn card.</returns>
    public ICard DrawOne(Deck drawDeck)
    {
        if (drawDeck.Cards.Count == 0) return null;
        return Draw(drawDeck, 1)[0];
    }

    /// <summary>
    /// Gets the positions of the given cards in the _allCards list.
    /// </summary>
    /// <param name="cards">The list of cards for which to get indices.</param>
    /// <returns>The list of indices.</returns>
    public List<int> GetCardIndices(List<ICard> cards)
    {
        List<int> cardIndices = new();

        foreach (ICard card in cards)
        {
            for (var i = 0; i < _allCards.Count; i++)
            {
                if (card == _allCards[i].Card) cardIndices.Add(i);
            }
        }

        return cardIndices;
    }

    /// <summary>
    /// Adds the given card to the relevant deck.
    /// </summary>
    /// <param name="card">The card to add.</param>
    public void AddToDeck(ICard card)
    {
        switch (card.Type)
        {
            case ICard.CardType.Core:    _coreDeck.AddCard(card); break;
            case ICard.CardType.Offence: _offenceDeck.AddCard(card); break;
            case ICard.CardType.Defence: _defenceDeck.AddCard(card); break;
            case ICard.CardType.Utility: _utilityDeck.AddCard(card); break;
        }
    }

    /// <summary>
    /// Instantiates the given cards, setting orientation, ownership and parents appropriately while adding them to the relevant player's hand.
    /// </summary>
    /// <param name="newCards">The cards to instantiate.</param>
    /// <param name="isPlayer1Cards">Whether to update the hand of player 1 (host).</param>
    /// <param name="player2ClientId">Client ID of player 2.</param>
    /// <param name="player1">Player 1 object (host).</param>
    /// <param name="player2">Player 2 object (client).</param>
    public async Task InstantiateCards(List<ICard> newCards, bool isPlayer1Cards, ulong player2ClientId, Player player1, Player player2)
    {
        List<GameObject> playerCards = isPlayer1Cards ? _player1Cards : _player2Cards;

        foreach (ICard card in newCards)
        {
            // Create play card
            GameObject cardObj = Instantiate(_cardPrefab);
            var cardObjData = cardObj.GetComponent<PlayCard>();

            playerCards.Add(cardObj);
            cardObj.GetComponent<NetworkObject>().Spawn(); // Spawn playcard on the network
            cardObjData.SetCardDataRpc(CardToCardIndex[card]); // Set playcard card data to the current card

            await Awaitable.NextFrameAsync();
        }

        await UpdateHand(isPlayer1Cards, player2ClientId, player1.transform, player2.transform);
    }

    /// <summary>
    /// Sorts the given cards.
    /// </summary>
    /// <param name="playerCards">The list of cards to sort.</param>
    public void SortPlayerCards(List<GameObject> playerCards)
    {
        playerCards.Sort((a, b) =>
        {
            ICard cardA = a.GetComponent<PlayCard>().CardData;
            ICard cardB = b.GetComponent<PlayCard>().CardData;

            return cardA.CompareTo(cardB);
        });
    }

    /// <summary>
    /// Sets the position of each card in the hand based on which player's hand it is.
    /// </summary>
    /// <param name="hand">The cards to spread.</param>
    /// <param name="isPlayer1Cards">Whether the hand is of player 1 (host).</param>
    public async Task SpreadCards(List<GameObject> hand, bool isPlayer1Cards)
    {
        var handPosY = isPlayer1Cards ? -5f : 5f;
        var minHandPosX = -5f;
        var maxHandPosX = 5f;
        var cardNum = hand.Count;
        Vector3 cardPos;

        for (var i = 0; i < cardNum; i++)
        {
            int sortingOrder = 0;

            // Calculate card position
            if (cardNum <= 1) cardPos = Vector3.zero;
            else
            {
                float maxValue = cardNum - 1;
                float p1CardsValue = i / maxValue;
                float p2CardsValue = (maxValue - i) / maxValue;
                float tValue = isPlayer1Cards ? p1CardsValue : p2CardsValue;
                float cardPosX = Mathf.Lerp(minHandPosX, maxHandPosX, tValue);
                cardPos = new Vector3(cardPosX, handPosY, 0f);

                // Set card layering
                sortingOrder = cardNum - 1 - i;
            }

            // Set card position and orientation
            var cardObj = hand[i];
            var playCard = cardObj.GetComponent<PlayCard>();
            var cardRot = cardObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            if (!isPlayer1Cards) cardRot = Quaternion.Euler(0f, 0f, 180f);

            playCard.FlipToRpc(isPlayer1Cards, !isPlayer1Cards);
            playCard.SetOrderRpc(sortingOrder);
            playCard.SetCardTransformRpc(cardPos, cardRot);
            playCard.ResetTransformRpc();

            await Awaitable.NextFrameAsync();
        }
    }

    /// <summary>
    /// Removes a card from a players hand.
    /// </summary>
    /// <param name="playerCards">The list of cards from which to remove the card.</param>
    /// <param name="cardObj">The card to remove.</param>
    public void RemoveFromPlayerHand(List<GameObject> playerCards, GameObject cardObj)
    {
        playerCards.Remove(cardObj);
    }

    /// <summary>
    /// Discards (removes from hand and returns to deck) the given card from the given hand.
    /// </summary>
    /// <param name="hand">The hand from which to discard the card.</param>
    /// <param name="playerCards">The list of cards from which to discard the card.</param>
    /// <param name="card">The card to discard.</param>
    public void DiscardCard(Deck hand, List<GameObject> playerCards, PlayCard card)
    {
        hand.Cards.Remove(card.CardData);
        playerCards.Remove(card.gameObject);
        AddToDeck(card.CardData);
        Destroy(card.gameObject);
    }

    /// <summary>
    /// Updates both players hands. 
    /// Note: Runs over multiple frames to keep the network from being overloaded.
    /// </summary>
    /// <param name="player2ClientId"></param>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    public async void UpdatePlayerCards(ulong player2ClientId, Transform player1, Transform player2)
    {
        await UpdateHand(true, player2ClientId, player1, player2);
        await UpdateHand(false, player2ClientId, player1, player2);
    }

    /// <summary>
    /// Swaps the hands of each player.
    /// </summary>
    public void SwapPlayerCards()
    {
        List<GameObject> tempHand = _player1Cards;

        _player1Cards = _player2Cards;
        _player2Cards = tempHand;
    }

    /// <summary>
    /// Handles the cleanup of placing a card.
    /// </summary>
    /// <param name="cardObj">The placed card object.</param>
    /// <param name="card">The placed play card.</param>
    /// <param name="cardData">The placed card's card data.</param>
    /// <param name="placingPlayer">The player who placed the card.</param>
    /// <param name="isPlayer1Turn">Whether it is player 1 (host)'s turn.</param>
    public void HandleCardPlaced(GameObject cardObj, PlayCard card, ICard cardData, Player placingPlayer, bool isPlayer1Turn)
    {
        // Handle core and peripheral cards
        if (cardData.Type == ICard.CardType.Core) card.FlipToRpc(false, false);
        else card.FlipToRpc(true, true);
        if (cardData.Type != ICard.CardType.Utility) placingPlayer.Hand.RemoveCard(cardData);

        // Handle hand
        RemoveFromPlayerHand(isPlayer1Turn ? Player1Cards : Player2Cards, cardObj);
        _ = SpreadCards(isPlayer1Turn ? Player1Cards : Player2Cards, isPlayer1Turn);
    }
}
