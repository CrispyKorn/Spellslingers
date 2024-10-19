using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class CardManager : NetworkBehaviour
{
    [System.Serializable]
    struct CardAmount
    {
        public ICard card;
        public int amount;

        public CardAmount(ICard _card, int _amount)
        {
            card = _card;
            amount = _amount;
        }
    }

    [System.Serializable]
    private struct NormalCardAmount
    {
        public Card card;
        public int amount;
    }

    [System.Serializable]
    private struct UtilityCardAmount
    {
        public UtilityCard card;
        public int amount;
    }

    [SerializeField] List<NormalCardAmount> allNormalCards = new List<NormalCardAmount>();
    [SerializeField] List<UtilityCardAmount> allUtilityCards = new List<UtilityCardAmount>();

    Deck offenceDeck, defenceDeck, coreDeck, utilityDeck;
    [SerializeField] GameObject cardPrefab;
    List<CardAmount> allCards;
    List<GameObject> player1Cards, player2Cards;

    public NetworkList<int> player1CardsIndices, player2CardsIndices;

    public List<GameObject> Player1Cards { get { return player1Cards; } }
    public List<GameObject> Player2Cards { get { return player2Cards; } }
    public Dictionary<int, ICard> CardIndexToCard { get; private set; }
    public Dictionary<ICard, int> CardToCardIndex { get; private set; }
    public Deck OffenceDeck { get { return offenceDeck; } }
    public Deck DefenceDeck { get { return defenceDeck; } }
    public Deck CoreDeck { get { return coreDeck; } }
    public Deck UtilityDeck { get { return utilityDeck; } }

    private void Awake()
    {
        player1CardsIndices = new NetworkList<int>();
        player2CardsIndices = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        allCards = new List<CardAmount>();
        player1Cards = new List<GameObject>();
        player2Cards = new List<GameObject>();

        foreach (NormalCardAmount cardAmount in allNormalCards)
        {
            allCards.Add(new CardAmount(cardAmount.card, cardAmount.amount));
        }

        foreach (UtilityCardAmount cardAmount in allUtilityCards)
        {
            allCards.Add(new CardAmount(cardAmount.card, cardAmount.amount));
        }

        CardIndexToCard = new Dictionary<int, ICard>();
        CardToCardIndex = new Dictionary<ICard, int>();

        for (int i = 0; i < allCards.Count; i++)
        {
            CardIndexToCard.Add(i, allCards[i].card);
            CardToCardIndex.Add(allCards[i].card, i);
        }

        offenceDeck = new Deck();
        defenceDeck = new Deck();
        coreDeck = new Deck();
        utilityDeck = new Deck();
    }

    public void PopulateDecks()
    {
        foreach (CardAmount cardAmount in allCards)
        {
            switch (cardAmount.card.Type)
            {
                case ICard.CardType.Offence:
                    {
                        for (int i = cardAmount.amount; i > 0; i--) offenceDeck.AddCard((Card)cardAmount.card);
                    }
                    break;
                case ICard.CardType.Defence:
                    {
                        for (int i = cardAmount.amount; i > 0; i--) defenceDeck.AddCard((Card)cardAmount.card);
                    }
                    break;
                case ICard.CardType.Core:
                    {
                        for (int i = cardAmount.amount; i > 0; i--) coreDeck.AddCard((Card)cardAmount.card);
                    }
                    break;
                case ICard.CardType.Utility:
                    {
                        if (cardAmount.card.CardName != "Smooth Talker") break;
                        for (int i = cardAmount.amount; i > 0; i--) utilityDeck.AddCard((UtilityCard)cardAmount.card);
                    }
                    break;
                default: break;
            }
        }
    }

    public List<ICard> Draw(Deck drawDeck, int numOfCards)
    {
        List<ICard> cards = new List<ICard>();
        for (int i = 0; i < numOfCards; i++)
        {
            if (drawDeck.cards.Count == 0) break;
            int cardToDrawIndex = Random.Range(0, drawDeck.cards.Count - 1);

            cards.Add(drawDeck.DrawCard(cardToDrawIndex));
        }

        return cards;
    }

    public List<int> GetCardIndices(List<ICard> cards)
    {
        List<int> cardIndices = new List<int>();

        foreach (ICard card in cards)
        {
            for (int i = 0; i < allCards.Count; i++)
            {
                if (card == allCards[i].card) cardIndices.Add(i);
            }
        }

        return cardIndices;
    }

    public void AddToDeck(ICard card)
    {
        switch (card.Type)
        {
            case ICard.CardType.Core:    coreDeck.AddCard(card); break;
            case ICard.CardType.Offence: offenceDeck.AddCard(card); break;
            case ICard.CardType.Defence: defenceDeck.AddCard(card); break;
            case ICard.CardType.Utility: utilityDeck.AddCard(card); break;
        }
    }

    public async Task InstantiateCards(List<ICard> newCards, bool isLocalPlayerCards, ulong player2ClientId, Player player1, Player player2)
    {
        List<GameObject> playerCards = isLocalPlayerCards ? player1Cards : player2Cards;

        foreach (ICard card in newCards)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            PlayCard cardObjData = cardObj.GetComponent<PlayCard>();

            cardObj.GetComponent<NetworkObject>().Spawn();

            cardObjData.SetCardDataClientRpc(CardToCardIndex[card]);

            if (isLocalPlayerCards) cardObjData.Flip();
            else cardObjData.FlipToClientRpc(true, false);
            if (!isLocalPlayerCards) cardObj.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            if (!isLocalPlayerCards) cardObjData.NetworkObject.ChangeOwnership(player2ClientId);

            cardObj.transform.SetParent(isLocalPlayerCards ? player1.transform : player2.transform);

            playerCards.Add(cardObj);

            await Task.Yield();
        }

        //Sort Hand
        if (isLocalPlayerCards) player1.hand.cards.Sort();
        else player2.hand.cards.Sort();

        SortPlayerCards(ref playerCards);

        await SpreadCards(playerCards, isLocalPlayerCards);
    }

    public void SortPlayerCards(ref List<GameObject> playerCards)
    {
        playerCards.Sort((a, b) =>
        {
            ICard cardA = a.GetComponent<PlayCard>().cardData;
            ICard cardB = b.GetComponent<PlayCard>().cardData;

            return cardA.CompareTo(cardB);
        });
    }

    public async Task SpreadCards(List<GameObject> hand, bool IsLocalPlayerCards)
    {
        float handPosY = IsLocalPlayerCards ? -5f : 5f;
        float minHandPosX = -5f;
        float maxHandPosX = 5f;
        int cardNum = hand.Count;
        Vector3 cardPos;

        for (int i = 0; i < cardNum; i++)
        {
            if (cardNum <= 1) cardPos = Vector3.zero;
            else
            {
                float tValue = IsLocalPlayerCards ? (float)i / (cardNum - 1) : (float)(cardNum - 1 - i) / (cardNum - 1);
                float cardPosX = Mathf.Lerp(minHandPosX, maxHandPosX, tValue);
                cardPos = new Vector3(cardPosX, handPosY, i);
            }
            PlayCard card = hand[i].GetComponent<PlayCard>();
            card.SetCardPosClientRpc(cardPos);
            card.ResetPosClientRpc();

            await Task.Yield();
        }
    }

    public void RemoveFromPlayerHand(List<GameObject> playerCards, GameObject cardObj)
    {
        playerCards.Remove(cardObj);
    }

    public void DiscardCard(Deck hand, List<GameObject> playerCards, PlayCard card)
    {
        hand.cards.Remove(card.cardData);
        playerCards.Remove(card.gameObject);
        AddToDeck(card.cardData);
        Destroy(card.gameObject);
    }

    public async void UpdatePlayerCards(ulong player2ClientId, Transform player1, Transform player2)
    {
        await UpdateHand(true, player2ClientId, player1, player2);
        await UpdateHand(false, player2ClientId, player1, player2);

        await SpreadCards(player1Cards, true);
        await SpreadCards(player2Cards, false);
    }

    private async Task UpdateHand(bool isLocalPlayerCards, ulong player2ClientId, Transform player1, Transform player2)
    {
        foreach (GameObject card in isLocalPlayerCards ? player1Cards : player2Cards)
        {
            PlayCard cardObjData = card.GetComponent<PlayCard>();

            cardObjData.FlipToClientRpc(false);
            if (isLocalPlayerCards) cardObjData.Flip();
            else cardObjData.FlipToClientRpc(true, false);
            if (isLocalPlayerCards) card.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            else card.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            if (!isLocalPlayerCards) cardObjData.NetworkObject.ChangeOwnership(player2ClientId);
            else cardObjData.NetworkObject.ChangeOwnership(NetworkManager.LocalClientId);

            card.transform.SetParent(isLocalPlayerCards ? player1 : player2);

            await Task.Yield();
        }
    }

    public void SwapPlayerCards()
    {
        List<GameObject> tempHand = player1Cards;

        player1Cards = player2Cards;
        player2Cards = tempHand;
    }
}
