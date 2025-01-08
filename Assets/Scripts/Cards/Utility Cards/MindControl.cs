using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Mind Control", fileName = "Mind_Control")]
public class MindControl : UtilityCard
{
    public override event Action<UtilityCard, bool, bool> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        GameBoard board = Locator.Instance.PlayManager.Board;
        GameObject coreCardObj = utilityInfo.ActivatedByPlayer1 ? board.Player2Board[(int)GameBoard.Slot.CoreSlot].Card : board.Player1Board[(int)GameBoard.Slot.CoreSlot].Card;
        PlayCard corePlayCard = coreCardObj.GetComponent<PlayCard>();

        // Check for invalid play
        if (coreCardObj == null || corePlayCard.IsFaceUp)
        {
            OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1, false);
            return;
        }

        corePlayCard.FlipToRpc(true, true);

        OnCardEffectComplete?.Invoke(this, utilityInfo.ActivatedByPlayer1, true);
    }
}
