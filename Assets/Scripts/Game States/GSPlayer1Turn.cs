using UnityEngine;

public class GSPlayer1Turn : GameState
{
    private int _cardsPlayed;

    public override void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        Debug.Log("Entering Player 1 Turn...");

        _stateManager = stateManager;
        _gameBoard = board;
        _cardsPlayed = 0;

        _gameBoard.player1Board[(int)GameBoard.Slot.CoreSlot].IsUsable = true;
        _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot1].IsUsable = false;
        _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot2].IsUsable = false;
        _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot3].IsUsable = false;
        _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot4].IsUsable = false;
        _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot5].IsUsable = false;

        foreach (CardSlot slot in _gameBoard.player2Board) slot.IsUsable = false;
    }

    public override void OnUpdateState()
    {
        Debug.Log("Updating Player 1 Turn...");

        _cardsPlayed++;

        switch (_cardsPlayed)
        {
            case 1:
                {
                    _gameBoard.player1Board[(int)GameBoard.Slot.CoreSlot].IsUsable = false;
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot1].IsUsable = true;
                }
                break;
            case 2:
                {
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot1].IsUsable = false;
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot2].IsUsable = true;
                }
                break;
            case 3:
                {
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot2].IsUsable = false;
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot3].IsUsable = true;
                }
                break;
            case 4:
                {
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot3].IsUsable = false;
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot4].IsUsable = true;
                }
                break;
            case 5:
                {
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot4].IsUsable = false;
                    _gameBoard.player1Board[(int)GameBoard.Slot.PeripheralSlot5].IsUsable = true;
                }
                break;
        }
    }
}
