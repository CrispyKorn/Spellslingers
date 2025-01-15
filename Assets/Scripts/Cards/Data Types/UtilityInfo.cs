
public class UtilityInfo
{
    public bool ActivatedByPlayer1 { get => _activatedByPlayer1; }
    public UtilityCard UtilityCard { get => _utilityCard;}
    public bool Successful { get => _successful; set => _successful = value; }

    private bool _activatedByPlayer1;
    private UtilityCard _utilityCard;
    private bool _successful;

    public UtilityInfo(bool activatedByPlayer1, UtilityCard utilityCard)
    {
        _activatedByPlayer1 = activatedByPlayer1;
        _utilityCard = utilityCard;
    }
}
