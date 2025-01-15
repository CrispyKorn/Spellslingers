using System.Collections.Generic;
using System;
using UnityEngine;

public class GameStateManager
{
    public enum GameStateIndex
    {
        Interrupt,
        Player1Turn,
        Player2Turn,
        Player1ExtendedTurn,
        Player2ExtendedTurn,
        Battle
    }

    public event Action<int> OnStateUpdated;
    public event Action<int> OnGameStateChanged;
    public event Action<bool> OnRoundEnd;

    public GSPlayerTurn Player1Turn { get => _player1Turn; }
    public GSPlayerTurn Player2Turn { get => _player2Turn; }
    public GSPlayerTurn GSPlayer1ExtendedTurn { get => _player1ExtendedTurn; }
    public GSPlayerTurn GSPlayer2ExtendedTurn { get => _player2ExtendedTurn; }
    public GSInterrupt Interrupt { get => _interrupt; }
    public GSBattle Battle { get => _battle; }
    public bool P1First { get => _p1First; }
    public GameState CurrentState { get => _currentState; }
    public int CurrentStateIndex { get => _stateIndices[_currentState]; }
    public GameState PrevState { get => _prevState; }
    public PlayManager PlayManager { get => _playManager; }
    public CardManager CardManager { get => _cardManager; }

    private GSPlayerTurn _player1Turn = new(true, false);
    private GSPlayerTurn _player2Turn = new(false, false);
    private GSPlayerTurn _player1ExtendedTurn = new(true, true);
    private GSPlayerTurn _player2ExtendedTurn = new(false, true);
    private GSInterrupt _interrupt = new();
    private GSBattle _battle = new();
    private GameState _currentState;
    private GameState _prevState;
    private PlayManager _playManager;
    private CardManager _cardManager;
    private bool _p1First = true;
    private bool _extendP1Turn;
    private bool _extendP2Turn;
    private Dictionary<GameState, int> _stateIndices = new();
    private GameBoard _board;

    public GameStateManager(PlayManager playManager, CardManager cardManager, GameBoard board)
    {
        Locator.Instance.RegisterInstance(this);

        _currentState = _interrupt;
        _playManager = playManager;
        _cardManager = cardManager;
        _board = board;

        _stateIndices.Add(_interrupt, 0);
        _stateIndices.Add(_player1Turn, 1);
        _stateIndices.Add(_player2Turn, 2);
        _stateIndices.Add(_player1ExtendedTurn, 3);
        _stateIndices.Add(_player2ExtendedTurn, 4);
        _stateIndices.Add(_battle, 5);
    }

    /// <summary>
    /// Updates the currently active state.
    /// </summary>
    public void UpdateState()
    {
        OnStateUpdated?.Invoke(_stateIndices[_currentState]);
        _currentState.OnUpdateState();
    }

    public void FinishState()
    {
        switch (_stateIndices[_currentState])
        {
            case 0: // Interrupt
                {
                    SetState(_prevState, false);
                }
                break;
            case 1: // Player 1 Turn
                {
                    if (_p1First) SetState(_player2Turn);
                    else if (!HandleExtendTurn()) EndRound();
                }
                break;
            case 2: // Player 2 Turn
                {
                    if (!_p1First) SetState(_player1Turn);
                    else if (!HandleExtendTurn()) EndRound();
                }
                break;
            case 3: // Player 1 Extended Turn
                {
                    if (_p1First && _extendP2Turn) SetState(_player2Turn);
                    else EndRound();
                }
                break;
            case 4: // Player 2 Extended Turn
                {
                    if (!_p1First && _extendP1Turn) SetState(_player1Turn);
                    else EndRound();
                }
                break;
            case 5: // Battle
                {
                    _playManager.HandleEndOfRound();
                    ChangeRound();
                    SetState(_p1First ? _player1Turn : _player2Turn);
                }
                break;
        }
    }

    /// <summary>
    /// Sets the current gamestate to the given state.
    /// </summary>
    /// <param name="state">The new state to set the game to.</param>
    /// <param name="board">The active game board.</param>
    /// <param name="resetState">Whether to run the OnEnterState method of the new state.</param>
    public void SetState(GameState state, bool resetState = true)
    {
        _prevState = _currentState;
        _currentState = state;

        if (resetState) _currentState.OnEnterState(this, _board);

        OnGameStateChanged?.Invoke(_stateIndices[_currentState]);
    }

    /// <summary>
    /// Checks for turn extension core cards.
    /// </summary>
    /// <returns>Whether there are turn extensions.</returns>
    private bool HandleExtendTurn()
    {
        var p1CorePlayCard = _board.Player1Board[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>();
        var p2CorePlayCard = _board.Player2Board[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>();

        if (!p1CorePlayCard.IsFaceUp)
        {
            p1CorePlayCard.FlipToRpc(true, true);
            p2CorePlayCard.FlipToRpc(true, true);

            var p1CoreCard = (CoreCard)p1CorePlayCard.CardData;
            var p2CoreCard = (CoreCard)p2CorePlayCard.CardData;
            bool extendP1Turn = p1CoreCard.IsNullfied ? false : p1CoreCard.CoreType == CoreCard.CoreCardType.TurnExtender;
            bool extendP2Turn = p2CoreCard.IsNullfied ? false : p2CoreCard.CoreType == CoreCard.CoreCardType.TurnExtender;

            if (extendP1Turn || extendP2Turn)
            {
                ExtendTurn(extendP1Turn, extendP2Turn);
                return true;
            }
        }

        return false;
    }

    private void ExtendTurn(bool extendP1Turn, bool extendP2Turn)
    {
        GameState nextState;
        _extendP1Turn = extendP1Turn;
        _extendP2Turn = extendP2Turn;

        if (_p1First)
        {
            if (_extendP1Turn) nextState = _player1ExtendedTurn;
            else nextState = _player2ExtendedTurn;
        }
        else
        {
            if (_extendP2Turn) nextState = _player2ExtendedTurn;
            else nextState = _player1ExtendedTurn;
        }

        SetState(nextState);
    }

    /// <summary>
    /// Ends a round, moving to the next round
    /// </summary>
    public async void EndRound()
    {
        await Awaitable.WaitForSecondsAsync(3);

        // Battle Phase Begin
        SetState(_battle);
    }

    /// <summary>
    /// Switches the attacking player and ends the round.
    /// </summary>
    public void ChangeRound()
    {
        _p1First = !_p1First;
        OnRoundEnd?.Invoke(_p1First);
    }
}
