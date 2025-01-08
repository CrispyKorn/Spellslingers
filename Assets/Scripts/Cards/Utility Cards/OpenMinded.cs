using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;

[CreateAssetMenu(menuName = "Utility Card/Open Minded", fileName = "Open_Minded")]
public class OpenMinded : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

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
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player1 : _utilityInfo.Player2;
        _uiManager = Locator.Instance.UIManager;
        _elementSelectionManager = _uiManager.ElementSelectionManager;
        _placingPlayerClientId = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.RelayManager.Player1ClientId : Locator.Instance.RelayManager.Player2ClientId;
        Debug.Log(_placingPlayerClientId);
        _placingPlayer.OnCardSelected += OnCardSelected;
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
        _placingPlayer.OnCardSelected -= OnCardSelected;
    }

    private void OnElementButtonClicked(Card.CardElement element)
    {
        // Disable element buttons
        _uiManager.SetElementSelectionActive(false, _placingPlayerClientId);
        _elementSelectionManager.OnElementButtonClicked -= OnElementButtonClicked;

        // Find card equivalent
        ICard.CardType cardType = _selectedCard.CardData.Type;
        Deck searchDeck = null;

        switch (cardType)
        {
            case ICard.CardType.Core: searchDeck = _utilityInfo.CardManager.CoreDeck; break;
            case ICard.CardType.Offence: searchDeck = _utilityInfo.CardManager.OffenceDeck; break;
            case ICard.CardType.Defence: searchDeck = _utilityInfo.CardManager.DefenceDeck; break;
        }

        ICard equivalentCard = _selectedCard.CardData;

        if (cardType == ICard.CardType.Core)
        {
            // Requires manual mapping
            List<CoreCardSet> coreCardSets = new();
            coreCardSets.Add(new CoreCardSet(typeof(TumblingAvalanche), typeof(MeteorShower), typeof(WrathOfTheHeavens)));
            coreCardSets.Add(new CoreCardSet(typeof(TidalWave), typeof(SpewingVolcano), typeof(VortexOfLightning)));
            coreCardSets.Add(new CoreCardSet(typeof(RagingTyphoon), typeof(WhippingWhirlwind), typeof(ChaoticThunderstorm)));
            coreCardSets.Add(new CoreCardSet(typeof(DelicateIcicle), typeof(WhisperingEmber), typeof(TeslaCoil)));
            coreCardSets.Add(new CoreCardSet(typeof(Riptide), typeof(Backdraft), typeof(StaticShock)));
            coreCardSets.Add(new CoreCardSet(typeof(ShardsOfPurity), typeof(PiercingSunbeams), typeof(FlickeringLightningBugs)));
            coreCardSets.Add(new CoreCardSet(typeof(RushingWaterfall), typeof(PillarOfFlame), typeof(SpearOfSpark)));
            coreCardSets.Add(new CoreCardSet(typeof(GurglingCauldron), typeof(RainbowRope), typeof(RadiantLightshow)));

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

        // Swap cards
        bool selectedCardIsFaceUp = _selectedCard.IsFaceUp;

        _selectedCardSlot.TakeCard();
        _utilityInfo.CardManager.DiscardCard(_selectedCard);
        _utilityInfo.CardManager.InstantiateCardToSlot(equivalentCard, _selectedCardSlot, selectedCardIsFaceUp);

        // Finish
        _placingPlayer.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, true);
    }
}
