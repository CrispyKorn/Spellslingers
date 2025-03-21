using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;

[CreateAssetMenu(menuName = "Utility Card/Open Minded", fileName = "Open_Minded")]
public class OpenMinded : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private struct CoreCardSet
    {
        public Type WaterCoreCard { get => _waterCoreCard; }
        public Type FireCoreCard { get => _fireCoreCard; }
        public Type ElectricityCoreCard { get => _electricityCoreCard; }

        private Type _waterCoreCard;
        private Type _fireCoreCard;
        private Type _electricityCoreCard;

        public CoreCardSet(Type waterCoreCard, Type fireCoreCard, Type electricityCoreCard)
        {
            _waterCoreCard = waterCoreCard;
            _fireCoreCard = fireCoreCard;
            _electricityCoreCard = electricityCoreCard;
        }
    }

    private UtilityInfo _utilityInfo;
    private Player _placingPlayer;
    private ulong _placingPlayerClientId;
    private PlayCard _selectedCard;
    private CardSlot _selectedCardSlot;
    private UIManager _uiManager;
    private ElementSelectionManager _elementSelectionManager;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayerManager.Player1 : Locator.Instance.PlayerManager.Player2;
        _uiManager = Locator.Instance.UIManager;
        _elementSelectionManager = _uiManager.ElementSelectionManager;
        _placingPlayerClientId = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.RelayManager.Player1ClientId : Locator.Instance.RelayManager.Player2ClientId;
        _placingPlayer.Interaction.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        _selectedCard = selectedCard;
        _selectedCardSlot = _selectedCard.PlacedCardSlot;

        // Check for invalid selection
        if (_selectedCardSlot == null || selectedCard.CardData.Type == ICard.CardType.Utility) return;

        // Activate selection buttons and await choice
        _uiManager.SetElementSelectionActive(true, _placingPlayerClientId);
        _elementSelectionManager.OnElementButtonClicked += OnElementButtonClicked;
        _placingPlayer.Interaction.OnCardSelected -= OnCardSelected;
    }

    private void OnElementButtonClicked(Card.CardElement element)
    {
        // Disable element buttons
        _uiManager.SetElementSelectionActive(false, _placingPlayerClientId);
        _elementSelectionManager.OnElementButtonClicked -= OnElementButtonClicked;

        // Find card equivalent
        CardManager cardManager = Locator.Instance.CardManager;
        ICard.CardType cardType = _selectedCard.CardData.Type;
        Deck searchDeck = null;

        switch (cardType)
        {
            case ICard.CardType.Core: searchDeck = cardManager.CoreDeck; break;
            case ICard.CardType.Offence: searchDeck = cardManager.OffenceDeck; break;
            case ICard.CardType.Defence: searchDeck = cardManager.DefenceDeck; break;
        }

        ICard equivalentCard = FindEquivalentCard(cardType, element, searchDeck);

        // Swap cards
        bool selectedCardIsFaceUp = _selectedCard.IsFaceUp;
        bool selectedCardOwnedByPlayer2 = !_selectedCard.IsOwnedByServer;

        _selectedCardSlot.TakeCard();
        cardManager.DiscardCard(_selectedCard);
        cardManager.InstantiateCardToSlot(equivalentCard, _selectedCardSlot, selectedCardIsFaceUp, selectedCardOwnedByPlayer2);

        // Finish
        _utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(_utilityInfo);
    }

    private ICard FindEquivalentCard(ICard.CardType cardType, Card.CardElement element, Deck searchDeck)
    {
        ICard equivalentCard = _selectedCard.CardData;

        if (cardType == ICard.CardType.Core)
        {
            // Requires manual mapping
            List<CoreCardSet> coreCardSets = new()
            {
                new CoreCardSet(typeof(TumblingAvalanche), typeof(MeteorShower), typeof(WrathOfTheHeavens)),
                new CoreCardSet(typeof(TidalWave), typeof(SpewingVolcano), typeof(VortexOfLightning)),
                new CoreCardSet(typeof(RagingTyphoon), typeof(WhippingWhirlwind), typeof(ChaoticThunderstorm)),
                new CoreCardSet(typeof(DelicateIcicle), typeof(WhisperingEmber), typeof(TeslaCoil)),
                new CoreCardSet(typeof(Riptide), typeof(Backdraft), typeof(StaticShock)),
                new CoreCardSet(typeof(ShardsOfPurity), typeof(PiercingSunbeams), typeof(FlickeringLightningBugs)),
                new CoreCardSet(typeof(RushingWaterfall), typeof(PillarOfFlame), typeof(SpearOfSpark)),
                new CoreCardSet(typeof(GurglingCauldron), typeof(RainbowRope), typeof(RadiantLightshow))
            };

            CoreCardSet selectionSet = coreCardSets.Find(set =>
            {
                Type cardType = _selectedCard.CardData.GetType();

                bool isWaterCoreType = set.WaterCoreCard == cardType;
                bool isFireCoreType = set.FireCoreCard == cardType;
                bool isElectricityCoreType = set.ElectricityCoreCard == cardType;

                return isWaterCoreType || isFireCoreType || isElectricityCoreType;
            });

            switch (element)
            {
                case Card.CardElement.Water: equivalentCard = searchDeck.Cards.Find(cardData => cardData.GetType() == selectionSet.WaterCoreCard); break;
                case Card.CardElement.Fire: equivalentCard = searchDeck.Cards.Find(cardData => cardData.GetType() == selectionSet.FireCoreCard); break;
                case Card.CardElement.Electricity: equivalentCard = searchDeck.Cards.Find(cardData => cardData.GetType() == selectionSet.ElectricityCoreCard); break;
            }
        }
        else
        {
            equivalentCard = searchDeck.Cards.Find(cardData =>
            {
                Card card = (Card)cardData;
                Card selectedCard = (Card)_selectedCard.CardData;
                bool sameValues = card.Values.Equals(selectedCard.Values);
                bool sameElement = card.Element == element;

                return sameValues && sameElement;
            });
        }

        return equivalentCard;
    }
}
