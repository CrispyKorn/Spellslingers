using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSPlayer2Turn : GameState
{
    int cardsPlayed = 0;

    public override void OnEnterState(GameStateManager _stateManager, GameBoard _board)
    {
        Debug.Log("Entering Player 2 Turn...");

        stateManager = _stateManager;
        gameBoard = _board;
        cardsPlayed = 0;

        gameBoard.player2Board[(int)GameBoard.Slot.CoreSlot].useable.Value = true;
        gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot1].useable.Value = false;
        gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot2].useable.Value = false;
        gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot3].useable.Value = false;
        gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot4].useable.Value = false;
        gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot5].useable.Value = false;

        foreach (CardSlot slot in _board.player1Board) slot.useable.Value = false;
    }

    public override void OnUpdateState()
    {
        Debug.Log("Updating Player 2 Turn...");

        cardsPlayed++;

        switch (cardsPlayed)
        {
            case 1:
                {
                    gameBoard.player2Board[(int)GameBoard.Slot.CoreSlot].useable.Value = false;
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot1].useable.Value = true;
                }
                break;
            case 2:
                {
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot1].useable.Value = false;
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot2].useable.Value = true;
                }
                break;
            case 3:
                {
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot2].useable.Value = false;
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot3].useable.Value = true;
                }
                break;
            case 4:
                {
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot3].useable.Value = false;
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot4].useable.Value = true;
                }
                break;
            case 5:
                {
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot4].useable.Value = false;
                    gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot5].useable.Value = true;
                }
                break;
        }
    }
}
