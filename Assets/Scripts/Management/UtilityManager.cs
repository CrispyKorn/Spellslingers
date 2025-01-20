using UnityEngine;

public class UtilityManager : MonoBehaviour
{
    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
    }

    /// <summary>
    /// Run when a utility card effect is complete. Cleans up the game state and cards.
    /// </summary>
    /// <param name="utilityCard">The utility card that was used.</param>
    /// <param name="activatedByPlayer1">Whether player 1 played the utility card.</param>
    /// <param name="successful">Whether the utility card applied its effect successfully.</param>
    private void UtilityCardCleanup(UtilityInfo utilityInfo)
    {
        Locator locator = Locator.Instance;

        Hand playerHand = utilityInfo.ActivatedByPlayer1 ? locator.PlayerManager.Player1.Hand : locator.PlayerManager.Player2.Hand;

        if (utilityInfo.Successful)
        {
            if (utilityInfo.UtilityCard.UtilityType == UtilityCard.UtilityCardType.Normal)
            {
                PlayCard utilityPlayCard = locator.GameBoard.UtilitySlot.TakeCard().GetComponent<PlayCard>();
                locator.CardManager.DiscardCard(utilityPlayCard);
            }
        }
        else locator.CardManager.GiveCardToPlayer(locator.GameBoard.UtilitySlot.TakeCard(), utilityInfo.ActivatedByPlayer1);

        locator.GameStateManager.UpdateState();
        utilityInfo.UtilityCard.OnCardEffectComplete -= UtilityCardCleanup;
    }

    /// <summary>
    /// Applies the effect of the given utility card.
    /// </summary>
    /// <param name="utilityInfo">The information about the activated utility effect.</param>
    public void ApplyUtilityEffect(UtilityInfo utilityInfo)
    {
        utilityInfo.UtilityCard.OnCardEffectComplete += UtilityCardCleanup;
        utilityInfo.UtilityCard.ApplyEffect(utilityInfo);
    }
}
