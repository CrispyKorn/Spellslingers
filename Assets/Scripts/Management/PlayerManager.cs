using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public Player Player1 { get => _player1; }
    public Player Player2 { get => _player2; }
    public ulong Player2ClientId { get => _player2ClientId; }

    private Player _player1;
    private Player _player2;
    private ulong _player2ClientId;
    private PlayManager _playManager;
    private UIManager _uiManager;

    private void Awake()
    {
        _playManager = GetComponent<PlayManager>();
        _uiManager = GetComponent<UIManager>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        RelayManager relayManager = Locator.Instance.RelayManager;
        _player2ClientId = relayManager.Player2ClientId;
        _player1 = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
        _player2 = NetworkManager.ConnectedClients[relayManager.Player2ClientId].PlayerObject.GetComponent<Player>();

        _player1.name = "Player 1";
        _player2.name = "Player 2";
        _player1.IsPlayer1 = true;
        _player2.IsPlayer1 = false;
    }

    /// <summary>
    /// Deals damage to the relevant player, handling all relevant tasks.
    /// </summary>
    /// <param name="player1Attacking">Whether player 1 is attacking this round.</param>
    /// <param name="damage">The amount of damage applied.</param>
    public void DealDamage(bool player1Attacking, int damage)
    {
        Player playerHurt = player1Attacking ? _player2 : _player1;
        playerHurt.Health -= damage;

        // Update UI
        _uiManager.UpdateUI(_player1.Health, _player2.Health, _playManager.CurrentGameState, IsHost);

        // Check Game Over
        if (playerHurt.Health <= 0) _playManager.GameOver(player1Attacking);
    }
}
