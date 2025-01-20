using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Utility Card/One Mans Trash", fileName = "One_Mans_Trash")]
public class OneMansTrash : UtilityCard
{
    public override event Action<UtilityInfo> OnCardEffectComplete;

    public override async void ApplyEffect(UtilityInfo utilityInfo)
    {
        // Apply effect
        await Locator.Instance.CardManager.SwapPlayerCards();

        // Finish
        utilityInfo.Successful = true;
        OnCardEffectComplete?.Invoke(utilityInfo);
    }
}
