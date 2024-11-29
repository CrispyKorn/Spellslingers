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

    private async Task UpdateHand(bool isLocalPlayerCards, ulong player2ClientId, Transform player1, Transform player2)
    {
        foreach (GameObject card in isLocalPlayerCards ? _player1Cards : _player2Cards)
        {
            var cardObjData = card.GetComponent<PlayCard>();

            // Set card orientation based on player
            cardObjData.FlipToClientRpc(false);
            if (isLocalPlayerCards) cardObjData.Flip();
            else cardObjData.FlipToClientRpc(true, false);
            if (isLocalPlayerCards) card.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            else card.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            // Set card ownership based on player
            if (!isLocalPlayerCards) cardObjData.NetworkObject.ChangeOwnership(player2ClientId);
            else cardObjData.NetworkObject.ChangeOwnership(NetworkManager.LocalClientId);

            card.transform.SetParent(isLocalPlayerCards ? player1 : player2);

            await Awaitable.NextFrameAsync();
        }
    }

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
                        if (cardData.Card.CardName != "Smooth Talker") break;
                        for (var i = cardData.Amount; i > 0; i--) _utilityDeck.AddCard((UtilityCard)cardData.Card);
                    }
                    break;
            }
        }
    }

    public void InitializePlayerCards(Player player1, Player player2, ulong player2ClientId)
    {
        InitializePlayerHand(player1.Hand, N_Player1CardsIndices);
        InitializePlayerHand(player2.Hand, N_Player2CardsIndices);

        _ = InstantiateCards(player1.Hand.Cards, true, player2ClientId, player1, player2);
        _ = InstantiateCards(player2.Hand.Cards, false, player2ClientId, player1, player2);
    }

    public async Task AddEndOfRoundCards(Player player1, Player player2, ulong player2ClientId)
    {
        List<ICard> player1Set = DrawEndOfRoundSet();
        List<ICard> player2Set = DrawEndOfRoundSet();

        player1.Hand.AddCards(player1Set);
        player2.Hand.AddCards(player2Set);

        await InstantiateCards(player1Set, true, player2ClientId, player1, player2);
        await InstantiateCards(player2Set, false, player2ClientId, player1, player2);
    }

    private List<ICard> DrawEndOfRoundSet()
    {
        var newCards = new List<ICard>();
        newCards.Add(DrawOne(OffenceDeck));
        newCards.Add(DrawOne(DefenceDeck));
        newCards.Add(DrawOne(CoreDeck));
        newCards.Add(DrawOne(UtilityDeck));

        return newCards;
    }

    public List<ICard> Draw(Deck drawDeck, int numOfCards)
    {
        List<ICard> cards = new();

        for (var i = 0; i < numOfCards; i++)
        {
            if (drawDeck.Cards.Count == 0) break;
            int cardToDrawIndex = UnityEngine.Random.Range(0, drawDeck.Cards.Count);

            cards.Add(drawDeck.DrawCard(cardToDrawIndex));
        }

        return cards;
    }

    public ICard DrawOne(Deck drawDeck)
    {
        return Draw(drawDeck, 1)[0];
    }

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

    public async Task InstantiateCards(List<ICard> newCards, bool isLocalPlayerCards, ulong player2ClientId, Player player1, Player player2)
    {
        List<GameObject> playerCards = isLocalPlayerCards ? _player1Cards : _player2Cards;

        foreach (ICard card in newCards)
        {
            // Create play card
            GameObject cardObj = Instantiate(_cardPrefab);
            var cardObjData = cardObj.GetComponent<PlayCard>();

            // Spawn playcard on the network
            cardObj.GetComponent<NetworkObject>().Spawn();

            // Set playcard card data to the current card
            cardObjData.SetCardDataClientRpc(CardToCardIndex[card]);

            // Cards spawn face down, so flip to face-up when they are ours
            if (isLocalPlayerCards) cardObjData.Flip();
            else
            {
                cardObjData.FlipToClientRpc(true, false);
                cardObj.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                cardObjData.NetworkObject.ChangeOwnership(player2ClientId);
            }

            cardObj.transform.SetParent(isLocalPlayerCards ? player1.transform : player2.transform);

            playerCards.Add(cardObj);

            await Awaitable.NextFrameAsync();
        }

        // Sort Hand
        if (isLocalPlayerCards) player1.Hand.Cards.Sort();
        else player2.Hand.Cards.Sort();

        SortPlayerCards(playerCards);

        await SpreadCards(playerCards, isLocalPlayerCards);
    }

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
    /// <param name="isLocalPlayerCards">Whether the hand is of the local player.</param>
    public async Task SpreadCards(List<GameObject> hand, bool isLocalPlayerCards)
    {
        var handPosY = isLocalPlayerCards ? -5f : 5f;
        var minHandPosX = -5f;
        var maxHandPosX = 5f;
        var cardNum = hand.Count;
        Vector3 cardPos;

        for (var i = 0; i < cardNum; i++)
        {
            // Calculate card position
            if (cardNum <= 1) cardPos = Vector3.zero;
            else
            {
                float maxValue = cardNum - 1;
                float localCardsValue = i / maxValue;
                float opponentCardsValue = (maxValue - i) / maxValue;
                float tValue = isLocalPlayerCards ? localCardsValue : opponentCardsValue;
                float cardPosX = Mathf.Lerp(minHandPosX, maxHandPosX, tValue);
                cardPos = new Vector3(cardPosX, handPosY, i);
            }

            // Set card position
            var card = hand[i].GetComponent<PlayCard>();
            card.SetCardPosClientRpc(cardPos);
            card.ResetPosClientRpc();

            await Awaitable.NextFrameAsync();
        }
    }

    public void RemoveFromPlayerHand(List<GameObject> playerCards, GameObject cardObj)
    {
        playerCards.Remove(cardObj);
    }

    public void DiscardCard(Deck hand, List<GameObject> playerCards, PlayCard card)
    {
        hand.Cards.Remove(card.CardData);
        playerCards.Remove(card.gameObject);
        AddToDeck(card.CardData);
        Destroy(card.gameObject);
    }

    public async void UpdatePlayerCards(ulong player2ClientId, Transform player1, Transform player2)
    {
        await UpdateHand(true, player2ClientId, player1, player2);
        await UpdateHand(false, player2ClientId, player1, player2);

        await SpreadCards(_player1Cards, true);
        await SpreadCards(_player2Cards, false);
    }

    public void SwapPlayerCards()
    {
        List<GameObject> tempHand = _player1Cards;

        _player1Cards = _player2Cards;
        _player2Cards = tempHand;
    }
}
