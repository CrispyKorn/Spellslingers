using UnityEngine;

public class GSPlayer2Turn : GameState
{
    int cardsPlayed;

    public override void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        Debug.Log("Entering Player 2 Turn...");

        _stateManager = stateManager;
        _gameBoard = board;
        cardsPlayed = 0;

        _gameBoard.player2Board[(int)GameBoard.Slot.CoreSlot].IsUsable = true;
        _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot1].IsUsable  = false;
        _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot2].IsUsable = false;
        _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot3].IsUsable = false;
        _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot4].IsUsable = false;
        _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot5].IsUsable = false;

        foreach (CardSlot slot in board.player1Board) slot.IsUsable = false;
    }

    public override void OnUpdateState()
    {
        Debug.Log("Updating Player 2 Turn...");

        cardsPlayed++;

        switch (cardsPlayed)
        {
            case 1:
                {
                    _gameBoard.player2Board[(int)GameBoard.Slot.CoreSlot].IsUsable = false;
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot1].IsUsable = true;
                }
                break;
            case 2:
                {
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot1].IsUsable = false;
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot2].IsUsable = true;
                }
                break;
            case 3:
                {
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot2].IsUsable = false;
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot3].IsUsable = true;
                }
                break;
            case 4:
                {
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot3].IsUsable = false;
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot4].IsUsable = true;
                }
                break;
            case 5:
                {
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot4].IsUsable = false;
                    _gameBoard.player2Board[(int)GameBoard.Slot.PeripheralSlot5].IsUsable = true;
                }
                break;
        }
    }
}
