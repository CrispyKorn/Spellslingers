using System.Collections.Generic;
using System;

public class GameStateManager
{
    public enum GameStateIndex
    {
        Player1Turn,
        Player2Turn,
        Interrupt,
        Battle
    }

    public event Action<int> OnGameStateChanged;
    public event Action<bool> OnRoundEnd;
    public event Action OnStateUpdated;

    public GSPlayer1Turn Player1Turn { get => _player1Turn; }
    public GSPlayer2Turn Player2Turn { get => _player2Turn; }
    public GSInterrupt Interrupt { get => _interrupt; }
    public GSBattle Battle { get => _battle; }
    public bool P1Attacking { get => _p1Attacking; }
    public GameState CurrentState { get => _currentState; }
    public GameState PrevState { get => _prevState; }
    public PlayManager PlayManager { get => _playManager; }
    public CardManager CardManager { get => _cardManager; }

    private GSPlayer1Turn _player1Turn = new();
    private GSPlayer2Turn _player2Turn = new();
    private GSInterrupt _interrupt = new();
    private GSBattle _battle = new();
    private GameState _currentState;
    private GameState _prevState;
    private PlayManager _playManager;
    private CardManager _cardManager;
    private bool _p1Attacking = true;
    private Dictionary<GameState, int> _stateIndices = new();

    public GameStateManager(PlayManager playManager, CardManager cardManager)
    {
        _currentState = _interrupt;
        _playManager = playManager;
        _cardManager = cardManager;

        _stateIndices.Add(_player1Turn, 0);
        _stateIndices.Add(_player2Turn, 1);
        _stateIndices.Add(_interrupt, 2);
        _stateIndices.Add(_battle, 3);
    }

    public void UpdateState()
    {
        OnStateUpdated?.Invoke();
        _currentState.OnUpdateState();
    }

    public void FlipTurn(GameBoard board, bool resetState = true)
    {
        if (_currentState == _player1Turn)
        {
            SetState(_player2Turn, board, resetState);
            return;
        }

        if (_currentState == _player2Turn)
        {
            SetState(_player1Turn, board, resetState);
            return;
        }

        if (_currentState == _interrupt)
        {
            if (_prevState == _player1Turn)
            {
                SetState(_player1Turn, board, resetState);
                return;
            }
            else
            {
                SetState(_player2Turn, board, resetState);
                return;
            }
        }
    }

    public void SetState(GameState state, GameBoard board, bool resetState = true)
    {
        _prevState = _currentState;
        _currentState = state;

        if (resetState) _currentState.OnEnterState(this, board);

        OnGameStateChanged?.Invoke(_stateIndices[_currentState]);
    }

    public void ChangeRound()
    {
        _p1Attacking = !_p1Attacking;
        OnRoundEnd?.Invoke(_p1Attacking);
    }
}
