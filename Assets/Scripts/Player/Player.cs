using UnityEngine;
using System;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public int Health { get => _health; set => _health = value; }
    public bool IsPlayer1 { get => _isPlayer1; set => _isPlayer1 = value; }
    public Hand Hand { get => _hand; }
    public PlayerInteraction Interaction { get => _interaction; }

    [SerializeField, Range(0, 100)] private int _health = 30;

    private bool _isPlayer1;
    private Hand _hand = new();
    private PlayerInteraction _interaction;

    private void Awake()
    {
        _interaction = GetComponent<PlayerInteraction>();
    }
}
