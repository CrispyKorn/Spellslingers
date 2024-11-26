using UnityEngine;

public class UtilityManager : MonoBehaviour
{
    UtilityInfo utilityInfo;

    public void Initialize(UtilityInfo _utilityInfo)
    {
        utilityInfo = _utilityInfo;
    }

    public void ApplyUtilityEffect(UtilityCard utilityCard, bool activatedByPlayer1)
    {
        utilityInfo.ActivatedByPlayer1 = activatedByPlayer1;
        utilityCard.ApplyEffect(utilityInfo);
    }
}
