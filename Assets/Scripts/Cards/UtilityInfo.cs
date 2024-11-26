
public class UtilityInfo
{
    public CardManager CardManager { get => _cardManager; }
    public Player Player1 { get => _player1; }
    public Player Player2 { get => _player2; }
    public bool ActivatedByPlayer1 { get => _activatedByPlayer1; set => _activatedByPlayer1 = value; }

    private CardManager _cardManager;
    private Player _player1;
    private Player _player2;
    private bool _activatedByPlayer1;

    public UtilityInfo(CardManager cardManager, Player player1, Player player2, bool activatedByPlayer1 = false)
    {
        _cardManager = cardManager;
        _player1 = player1;
        _player2 = player2;
        _activatedByPlayer1 = activatedByPlayer1;
    }
}
