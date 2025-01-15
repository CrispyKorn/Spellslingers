using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Utility Card/Single Minded", fileName = "Single_Minded")]
public class SingleMinded : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private UIManager _uiManager;
    private CardManager _cardManager;
    private PeripheralSelectionManager _peripheralSelectionManager;
    private ulong _placingPlayerClientId;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _uiManager = Locator.Instance.UIManager;
        _cardManager = Locator.Instance.CardManager;
        _peripheralSelectionManager = _uiManager.PeripheralSelectionManager;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayerManager.Player1 : Locator.Instance.PlayerManager.Player2;
        _placingPlayerClientId = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.RelayManager.Player1ClientId : Locator.Instance.RelayManager.Player2ClientId;
        
        _uiManager.SetPeripheralSelectionActive(true, _placingPlayerClientId);
        _peripheralSelectionManager.OnPeripheralTypeSelected += OnPeripheralTypeSelected;
    }

    private async void OnPeripheralTypeSelected(bool offenceSelected)
    {
        // Disable selection buttons
        _uiManager.SetPeripheralSelectionActive(false, _placingPlayerClientId);
        _peripheralSelectionManager.OnPeripheralTypeSelected -= OnPeripheralTypeSelected;

        // Remove cards
        Hand playerHand = _placingPlayer.Hand;
        Deck chosenDeck = offenceSelected ? _cardManager.OffenceDeck : _cardManager.DefenceDeck;

        int numOfCardsToReplace = RemoveCardTypeFromHand(playerHand, !offenceSelected);

        // Replace with selected card type
        List<ICard> replacementCards = _cardManager.Draw(chosenDeck, numOfCardsToReplace);
        await _cardManager.InstantiateCards(replacementCards, _utilityInfo.ActivatedByPlayer1);

        //Finish
        _utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(_utilityInfo);
    }

    private int RemoveCardTypeFromHand(Hand playerHand, bool removeOffenceCards)
    {
        ICard.CardType cardType = removeOffenceCards ? ICard.CardType.Offence : ICard.CardType.Defence;
        int numOfCardsRemoved = 0;

        for (int i = playerHand.Size - 1; i >= 0; i--)
        {
            GameObject currentCard = playerHand.CardObjs[i];
            if (playerHand.GetCardFromObj(currentCard).Type == cardType)
            {
                playerHand.RemoveCard(currentCard);
                _cardManager.DiscardCard(currentCard.GetComponent<PlayCard>());
                numOfCardsRemoved++;
            }
        }

        return numOfCardsRemoved;
    }
}
