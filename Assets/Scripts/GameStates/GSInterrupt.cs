using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSInterrupt : GameState
{
    public override void OnEnterState(GameStateManager _stateManager, GameBoard _board)
    {
        stateManager = _stateManager;
        gameBoard = _board;

        foreach (CardSlot slot in gameBoard.player1Board) slot.useable.Value = false;
        foreach (CardSlot slot in gameBoard.player2Board) slot.useable.Value = false;
    }

    public override void OnUpdateState()
    {
        
    }
}
