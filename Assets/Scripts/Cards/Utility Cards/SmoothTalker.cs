using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Utility Card/Smooth Talker", fileName = "Smooth_Talker")]
public class SmoothTalker : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    private UtilityInfo _utilityInfo;
    private CardManager _cardManager;
    private Player _opponent;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        _utilityInfo = utilityInfo;
        _cardManager = Locator.Instance.CardManager;
        _opponent = _utilityInfo.ActivatedByPlayer1 ? Locator.Instance.PlayerManager.Player2 : Locator.Instance.PlayerManager.Player1;

        if (!Array.Exists(_opponent.Hand.CardObjs, (o) => o.GetComponent<PlayCard>().CardData.Type == ICard.CardType.Offence 
                                                        || o.GetComponent<PlayCard>().CardData.Type == ICard.CardType.Defence))
        {
            OnCardEffectComplete?.Invoke(utilityInfo);
            return;
        }

        _cardManager.SetCardHighlights(true, !_utilityInfo.ActivatedByPlayer1, ICard.CardType.Offence, ICard.CardType.Defence);

        _opponent.Interaction.OnCardSelected += OnCardSelected;
    }

    private void OnCardSelected(Player selectingPlayer, PlayCard selectedCard)
    {
        // Check for invalid selections
        bool cardIsNotOpponents = !_opponent.Hand.CardObjs.Contains(selectedCard.gameObject);
        bool cardIsNotPeripheral = selectedCard.CardData.Type == ICard.CardType.Core || selectedCard.CardData.Type == ICard.CardType.Utility;

        if (cardIsNotOpponents || cardIsNotPeripheral) return;

        // Give the selected card to the activating player
        _cardManager.RemoveCardFromPlayer(selectedCard.gameObject, !_utilityInfo.ActivatedByPlayer1);
        _cardManager.GiveCardToPlayer(selectedCard.gameObject, _utilityInfo.ActivatedByPlayer1);

        // Finish
        _cardManager.SetCardHighlights(false, !_utilityInfo.ActivatedByPlayer1);
        _opponent.Interaction.OnCardSelected -= OnCardSelected;
        _utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(_utilityInfo);
    }
}
