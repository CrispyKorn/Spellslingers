using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Utility Card/Smooth Talker", fileName = "Smooth_Talker")]
public class SmoothTalker : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private Player _opponent;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _opponent = _utilityInfo.ActivatedByPlayer1 ? _utilityInfo.Player2 : _utilityInfo.Player1;

        if (!Array.Exists(_opponent.Hand.CardObjs, (o) => o.GetComponent<PlayCard>().CardData.Type == ICard.CardType.Offence 
                                                        || o.GetComponent<PlayCard>().CardData.Type == ICard.CardType.Defence))
        {
            OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, false);
            return;
        }

        _opponent.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        // Check for invalid selections
        bool cardIsNotOpponents = !_opponent.Hand.CardObjs.Contains(selectedCard.gameObject);
        bool cardIsNotPeripheral = selectedCard.CardData.Type == ICard.CardType.Core || selectedCard.CardData.Type == ICard.CardType.Utility;

        if (cardIsNotOpponents || cardIsNotPeripheral) return;

        // Give the selected card to the activating player
        _utilityInfo.CardManager.RemoveCardFromPlayer(selectedCard.gameObject, !_utilityInfo.ActivatedByPlayer1);
        _utilityInfo.CardManager.GiveCardToPlayer(selectedCard.gameObject, _utilityInfo.ActivatedByPlayer1);

        // Finish
        _opponent.OnCardSelected -= OnCardSelected;
        OnCardEffectComplete?.Invoke(this, _utilityInfo.ActivatedByPlayer1, true);
    }
}
