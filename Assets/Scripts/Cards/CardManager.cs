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

    public Dictionary<int, ICard> CardIndexToCard { get => _cardIndexToCard; }
    public Dictionary<ICard, int> CardToCardIndex { get => _cardToCardIndex; }
    public Deck OffenceDeck { get => _offenceDeck; }
    public Deck DefenceDeck { get => _defenceDeck; }
    public Deck CoreDeck { get => _coreDeck; }
    public Deck UtilityDeck { get => _utilityDeck; }

    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<CardData<Card>> _allNormalCards = new();
    [SerializeField] private List<CardData<UtilityCard>> _allUtilityCards = new();

    private Dictionary<int, ICard> _cardIndexToCard = new();
    private Dictionary<ICard, int> _cardToCardIndex = new();
    private Deck _offenceDeck = new();
    private Deck _defenceDeck = new();
    private Deck _coreDeck = new();
    private Deck _utilityDeck = new();
    private List<CardData<ICard>> _allCards = new();
    private PlayerManager _playerManager;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);

        _playerManager = GetComponent<PlayerManager>();
    }

    #region Private
    /// <summary>
    /// Populates various lists for card management.
    /// </summary>
    private void InitializeCardSets()
    {
        foreach (CardData<Card> cardAmount in _allNormalCards)
        {
            var newCard = new CardData<ICard>(cardAmount.Card, cardAmount.Amount);
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
    /// Initializes the given card.
    /// </summary>
    /// <param name="card">The card to instantiate.</param>
    /// <returns>The instantiated card's GameObject representation.</returns>
    private GameObject InitializeCard(ICard card)
    {
        // Create play card
        GameObject cardObj = Instantiate(_cardPrefab);
        var playCard = cardObj.GetComponent<PlayCard>();

        cardObj.GetComponent<NetworkObject>().Spawn(); // Spawn playcard on the network
        playCard.SetCardDataRpc(CardToCardIndex[card]); // Set playcard card data to the current card

        return cardObj;
    }

    /// <summary>
    /// Sets up a players hand of cards with proper orientation, ownership and parent.
    /// Note: This runs over a number of frames to keep the network from being overloaded with calls.
    /// </summary>
    /// <param name="isPlayer1Cards">Whether to update the hand of player 1 (host).</param>
    private async Task UpdateHand(bool isPlayer1Cards)
    {
        Player player1 = _playerManager.Player1;
        Player player2 = _playerManager.Player2;
        Hand playerHand = isPlayer1Cards ? player1.Hand : player2.Hand;

        foreach (GameObject cardObj in playerHand.CardObjs)
        {
            var playCard = cardObj.GetComponent<PlayCard>();

            // Set card ownership based on player (host is same as server)
            bool cardOwnedByServer = playCard.NetworkObject.IsOwnedByServer;
            if (isPlayer1Cards && !cardOwnedByServer) playCard.NetworkObject.ChangeOwnership(Locator.Instance.RelayManager.Player1ClientId);
            if (!isPlayer1Cards && cardOwnedByServer) playCard.NetworkObject.ChangeOwnership(Locator.Instance.RelayManager.Player2ClientId);

            cardObj.transform.SetParent(isPlayer1Cards ? player1.transform : player2.transform);

            await Awaitable.NextFrameAsync();
        }

        // Tidy Hand
        playerHand.SortHand();
        await SpreadCards(playerHand, isPlayer1Cards);
    }

    /// <summary>
    /// Draws the starting set of cards.
    /// </summary>
    /// <returns>The list of cards drawn.</returns>
    private List<ICard> GetInitialPlayerCards()
    {
        List<ICard> cards = new();
        cards.AddRange(Draw(CoreDeck, 3));
        cards.AddRange(Draw(OffenceDeck, 6));
        cards.AddRange(Draw(DefenceDeck, 6));
        cards.AddRange(Draw(UtilityDeck, 2));

        return cards;
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
    /// Completely moves all the cards from a hand into another.
    /// </summary>
    /// <param name="sourceHand">The hand to move cards from.</param>
    /// <param name="targetHand">The hand to move cards to.</param>
    private void MoveHand(Hand sourceHand, Hand targetHand)
    {
        int sourceHandSize = sourceHand.Size;

        for (int i = 0; i < sourceHandSize; i++)
        {
            GameObject cardObj = sourceHand.CardObjs[i];

            targetHand.AddCard(sourceHand.GetCardFromObj(cardObj), cardObj);
            sourceHand.RemoveCard(cardObj);
        }
    }

    /// <summary>
    /// Sets the position of each card in the hand based on which player's hand it is.
    /// </summary>
    /// <param name="hand">The cards to spread.</param>
    /// <param name="isPlayer1Cards">Whether the hand is of player 1 (host).</param>
    private async Task SpreadCards(Hand hand, bool isPlayer1Cards)
    {
        var handPosY = isPlayer1Cards ? -5f : 5f;
        var minHandPosX = -5f;
        var maxHandPosX = 5f;
        var cardNum = hand.Size;
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
            var cardObj = hand.CardObjs[i];
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
    #endregion

    #region Public
    public override void OnNetworkSpawn()
    {
        InitializeCardSets();
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
                        if (cardData.Card.CardName != "What Spell?") break;
                        for (var i = cardData.Amount; i > 0; i--) _utilityDeck.AddCard((UtilityCard)cardData.Card);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Initializes both players' hands with live cards.
    /// </summary>
    public async void InitializePlayerCards()
    {
        List<ICard> player1Cards = GetInitialPlayerCards();
        List<ICard> player2Cards = GetInitialPlayerCards();

        await InstantiateCards(player1Cards, true);
        await InstantiateCards(player2Cards, false);
    }

    /// <summary>
    /// Adds the relevant cards to each players hand for the end of a round.
    /// </summary>
    public async Task AddEndOfRoundCards()
    {
        List<ICard> player1Set = DrawEndOfRoundSet();
        List<ICard> player2Set = DrawEndOfRoundSet();

        await InstantiateCards(player1Set, true);
        await InstantiateCards(player2Set, false);
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

    public void InstantiateCardToSlot(ICard newCard, CardSlot cardSlot, bool faceUp)
    {
        GameObject cardObj = InitializeCard(newCard);
        cardSlot.TryPlaceCard(cardObj, true);
        cardObj.GetComponent<PlayCard>().FlipToRpc(faceUp, faceUp);
    }

    /// <summary>
    /// Instantiates the given cards, setting orientation, ownership and parents appropriately while adding them to the relevant player's hand.
    /// </summary>
    /// <param name="newCards">The cards to instantiate.</param>
    /// <param name="isPlayer1Cards">Whether the cards belong to player 1 (host).</param>
    public async Task InstantiateCards(List<ICard> newCards, bool isPlayer1Cards)
    {
        Hand playerHand = isPlayer1Cards ? _playerManager.Player1.Hand : _playerManager.Player2.Hand;

        foreach (ICard card in newCards)
        {
            GameObject cardObj = InitializeCard(card);
            playerHand.AddCard(card, cardObj); // Add to hand

            await Awaitable.NextFrameAsync();
        }

        await UpdateHand(isPlayer1Cards);
    }

    /// <summary>
    /// Give a card to a players hand.
    /// </summary>
    /// <param name="cardObj">The card to add.</param>
    /// <param name="giveToPlayer1">Whether to give the card to player 1 (host)'s hand.</param>
    public void GiveCardToPlayer(GameObject cardObj, bool giveToPlayer1)
    {
        Hand playerHand = giveToPlayer1 ? _playerManager.Player1.Hand : _playerManager.Player2.Hand;

        playerHand.AddCard(cardObj.GetComponent<PlayCard>().CardData, cardObj);
        _ = UpdateHand(giveToPlayer1);
    }

    /// <summary>
    /// Removes a card from a player's hand.
    /// </summary>
    /// <param name="cardObj">The card to remove.</param>
    /// <param name="removeFromPlayer1">Whether to remove the card from player 1 (host)'s hand.</param>
    public void RemoveCardFromPlayer(GameObject cardObj, bool removeFromPlayer1)
    {
        Player holdingPlayer = removeFromPlayer1 ? _playerManager.Player1 : _playerManager.Player2;
        Hand playerHand = holdingPlayer.Hand;

        playerHand.RemoveCard(cardObj);
        _ = SpreadCards(playerHand, removeFromPlayer1);
    }

    /// <summary>
    /// Returns the given card to the deck and destroys its GameObject representation.
    /// </summary>
    /// <param name="card">The card to discard.</param>
    public void DiscardCard(PlayCard card)
    {
        AddToDeck(card.CardData);
        card.GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// Updates both players hands. 
    /// Note: Runs over multiple frames to keep the network from being overloaded.
    /// </summary>
    public async void UpdatePlayerCards()
    {
        await UpdateHand(true);
        await UpdateHand(false);
    }

    /// <summary>
    /// Swaps the hands of each player.
    /// </summary>
    public void SwapPlayerCards()
    {
        Hand p1Hand = _playerManager.Player1.Hand;
        Hand p2Hand = _playerManager.Player2.Hand;
        Hand tempHand = new();

        MoveHand(p1Hand, tempHand);
        MoveHand(p2Hand, p1Hand);
        MoveHand(tempHand, p1Hand);

        UpdatePlayerCards();
    }
    #endregion
}
