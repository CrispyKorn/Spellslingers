using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager
{
    public enum GameStateIndex
    {
        Player1Turn,
        Player2Turn,
        Interrupt,
        Battle
    }

    GameState currentState;
    GameState prevState;
    PlayManager playManager;
    CardManager cardManager;
    bool p1Attacking = true;
    Dictionary<GameState, int> stateIndices;

    public GSPlayer1Turn player1Turn = new GSPlayer1Turn();
    public GSPlayer2Turn player2Turn = new GSPlayer2Turn();
    public GSInterrupt interrupt = new GSInterrupt();
    public GSBattle battle = new GSBattle();

    public event System.Action<int> OnGameStateChanged;
    public event System.Action<bool> OnRoundEnd;
    public event System.Action OnStateUpdated;

    public bool P1Attacking { get { return p1Attacking; } }
    public GameState CurrentState { get { return currentState; } }
    public GameState PrevState { get { return prevState; } }
    public PlayManager PlayManager { get { return playManager; } }
    public CardManager CardManager { get { return cardManager; } }

    public GameStateManager(PlayManager _playManager, CardManager _cardManager)
    {
        currentState = interrupt;
        playManager = _playManager;
        cardManager = _cardManager;

        stateIndices = new Dictionary<GameState, int>();
        stateIndices.Add(player1Turn, 0);
        stateIndices.Add(player2Turn, 1);
        stateIndices.Add(interrupt, 2);
        stateIndices.Add(battle, 3);
    }

    public void UpdateState()
    {
        OnStateUpdated?.Invoke();
        currentState.OnUpdateState();
    }

    public void FlipTurn(GameBoard _board, bool resetState = true)
    {
        if (currentState == player1Turn)
        {
            SetState(player2Turn, _board, resetState);
            return;
        }

        if (currentState == player2Turn)
        {
            SetState(player1Turn, _board, resetState);
            return;
        }

        if (currentState == interrupt)
        {
            if (prevState == player1Turn)
            {
                SetState(player1Turn, _board, resetState);
                return;
            }
            else
            {
                SetState(player2Turn, _board, resetState);
                return;
            }
        }
    }

    public void SetState(GameState _state, GameBoard _board, bool resetState = true)
    {
        prevState = currentState;
        currentState = _state;

        if (resetState) currentState.OnEnterState(this, _board);

        OnGameStateChanged?.Invoke(stateIndices[currentState]);
    }

    public void ChangeRound()
    {
        p1Attacking = !p1Attacking;
        OnRoundEnd?.Invoke(p1Attacking);
    }
}
