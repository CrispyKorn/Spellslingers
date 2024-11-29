using UnityEngine;
using Unity.Netcode;
using System;

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

        TrackPlayerHealthClientRpc();
    }

    [ClientRpc]
    public void TrackPlayerHealthClientRpc()
    {
        _player1.N_Health.OnValueChanged += OnPlayerHealthChanged;
        _player2.N_Health.OnValueChanged += OnPlayerHealthChanged;
    }

    private void OnPlayerHealthChanged(int previous, int current)
    {
        if (IsHost) _playManager.UpdateUIClientRpc();
    }

    public void DealDamage(bool player1Attacking, int damage)
    {
        Player playerHurt = player1Attacking ? _player2 : _player1;
        playerHurt.Health -= damage;

        // Update UI
        _uiManager.UpdateUI(_player1.Health, _player2.Health, _playManager.CurrentGameState);

        // Check Game Over
        if (playerHurt.Health <= 0) _playManager.GameOver(player1Attacking);
    }
}
