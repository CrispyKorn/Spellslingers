using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public Player Player1 { get => _player1; }
    public Player Player2 { get => _player2; }

    private Player _player1;
    private Player _player2;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        _player1 = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
        _player2 = NetworkManager.ConnectedClients[Locator.Instance.RelayManager.Player2ClientId].PlayerObject.GetComponent<Player>();

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
        Locator.Instance.UIManager.UpdateUIHealthRpc(_player1.Health, _player2.Health);

        // Check Game Over
        if (playerHurt.Health <= 0) Locator.Instance.PlayManager.GameOver(player1Attacking);
    }
}
