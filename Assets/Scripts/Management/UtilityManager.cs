using UnityEngine;

public class UtilityManager : MonoBehaviour
{
    UtilityInfo utilityInfo;

    /// <summary>
    /// Sets up the UtilityManager with the given UtilityInfo
    /// </summary>
    /// <param name="_utilityInfo">The info to setup with.</param>
    public void Initialize(UtilityInfo _utilityInfo)
    {
        utilityInfo = _utilityInfo;
    }

    /// <summary>
    /// Applies the effect of the given utility card.
    /// </summary>
    /// <param name="utilityCard">The utility card to apply.</param>
    /// <param name="activatedByPlayer1">Whether the utility card was placed by player 1 (host).</param>
    public void ApplyUtilityEffect(UtilityCard utilityCard, bool activatedByPlayer1)
    {
        utilityInfo.ActivatedByPlayer1 = activatedByPlayer1;
        utilityCard.ApplyEffect(utilityInfo);
    }
}
