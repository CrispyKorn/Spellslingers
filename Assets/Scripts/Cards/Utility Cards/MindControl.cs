using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/Mind Control", fileName = "Mind_Control")]
public class MindControl : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    public override void ApplyEffect(UtilityInfo utilityInfo)
    {
        GameBoard board = Locator.Instance.GameBoard;
        GameObject coreCardObj = utilityInfo.ActivatedByPlayer1 ? board.Player2Board[(int)GameBoard.Slot.CoreSlot].Card : board.Player1Board[(int)GameBoard.Slot.CoreSlot].Card;
        PlayCard corePlayCard;

        // Check for invalid play
        if (coreCardObj == null)
        {
            OnCardEffectComplete?.Invoke(utilityInfo);
            return;
        }

        corePlayCard = coreCardObj.GetComponent<PlayCard>();
        
        if (corePlayCard.IsFaceUp)
        {
            OnCardEffectComplete?.Invoke(utilityInfo);
            return;
        }

        // Activate effect
        corePlayCard.FlipToRpc(true, true);

        // Finish
        utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(utilityInfo);
    }
}
