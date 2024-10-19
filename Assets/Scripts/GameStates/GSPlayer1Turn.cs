using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSPlayer1Turn : GameState
{
    int cardsPlayed = 0;

    public override void OnEnterState(GameStateManager _stateManager, GameBoard _board)
    {
        Debug.Log("Entering Player 1 Turn...");

        stateManager = _stateManager;
        gameBoard = _board;
        cardsPlayed = 0;

        gameBoard.player1Board[(int)GameBoard.Slot.CoreSlot].useable.Value = true;
        gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot1].useable.Value = false;
        gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot2].useable.Value = false;
        gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot3].useable.Value = false;
        gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot4].useable.Value = false;
        gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot5].useable.Value = false;

        foreach (CardSlot slot in gameBoard.player2Board) slot.useable.Value = false;
    }

    public override void OnUpdateState()
    {
        Debug.Log("Updating Player 1 Turn...");

        cardsPlayed++;

        switch (cardsPlayed)
        {
            case 1:
                {
                    gameBoard.player1Board[(int)GameBoard.Slot.CoreSlot].useable.Value = false;
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot1].useable.Value = true;
                }
                break;
            case 2:
                {
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot1].useable.Value = false;
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot2].useable.Value = true;
                }
                break;
            case 3:
                {
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot2].useable.Value = false;
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot3].useable.Value = true;
                }
                break;
            case 4:
                {
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot3].useable.Value = false;
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot4].useable.Value = true;
                }
                break;
            case 5:
                {
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot4].useable.Value = false;
                    gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot5].useable.Value = true;
                }
                break;
        }
    }
}
