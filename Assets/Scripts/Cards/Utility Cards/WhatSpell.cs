using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/What Spell", fileName = "What_Spell")]
public class WhatSpell : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        // Check for invalid play
        bool allPlayer1BoardSlotsEmpty = !Array.Exists(Locator.Instance.GameBoard.Player1Board, slot => slot.HasCard);
        bool allPlayer2BoardSlotsEmpty = !Array.Exists(Locator.Instance.GameBoard.Player2Board, slot => slot.HasCard);
        if (allPlayer1BoardSlotsEmpty && allPlayer2BoardSlotsEmpty)
        {
            OnCardEffectComplete?.Invoke(_utilityInfo);
            return;
        }

        _utilityInfo = utilityInfo;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayerManager.Player1 : Locator.Instance.PlayerManager.Player2;

        _placingPlayer.Interaction.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedPlayCard)
    {
        // Check for invalid selection
        if (selectedPlayCard.PlacedCardSlot == null || selectedPlayCard.CardData.Type == ICard.CardType.Utility) return;

        // Valid selection, nullify card
        Card selectedCard = (Card)selectedPlayCard.CardData;
        selectedCard.IsNullfied = true;
        Debug.Log($"{selectedCard.CardName} has been nullified.");

        // Finish
        _placingPlayer.Interaction.OnCardSelected -= OnCardSelected;
        _utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(_utilityInfo);
    }
}
