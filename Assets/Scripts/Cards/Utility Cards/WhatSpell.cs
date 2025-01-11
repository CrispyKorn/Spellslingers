using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/What Spell", fileName = "What_Spell")]
public class WhatSpell : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private Player _placingPlayer;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _placingPlayer = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayerManager.Player1 : Locator.Instance.PlayerManager.Player2;

        _placingPlayer.OnCardSelected += OnCardSelected;
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
        _placingPlayer.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, true);
    }
}
