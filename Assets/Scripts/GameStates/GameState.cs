using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState
{
    protected GameStateManager stateManager;
    protected GameBoard gameBoard;

    public abstract void OnEnterState(GameStateManager _stateManager, GameBoard _board);
    public abstract void OnUpdateState();
}
