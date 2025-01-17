using UnityEngine;

public class GSPlayerTurn : GameState
{
    private bool _isP1Turn;
    private bool _isExtendedTurn;
    private CardSlot[] _myBoard;
    private CardSlot[] _opponentBoard;

    public GSPlayerTurn(bool isP1Turn, bool isExtendedTurn)
    {
        _isP1Turn = isP1Turn;
        _isExtendedTurn = isExtendedTurn;
    }

    public override void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        Debug.Log($"Entering Player {(_isP1Turn ? 1 : 2)} Turn...");

        _stateManager = stateManager;
        _gameBoard = board;

        if (_isP1Turn)
        {
            _myBoard = _gameBoard.Player1Board;
            _opponentBoard = _gameBoard.Player2Board;
        }
        else
        {
            _myBoard = _gameBoard.Player2Board;
            _opponentBoard = _gameBoard.Player1Board;
        }

        board.UtilitySlot.SetUsable(true);

        _myBoard[(int)GameBoard.Slot.CoreSlot].SetUsable(!_isExtendedTurn);
        _myBoard[(int)GameBoard.Slot.PeripheralSlot1].SetUsable(false);
        _myBoard[(int)GameBoard.Slot.PeripheralSlot2].SetUsable(false);
        _myBoard[(int)GameBoard.Slot.PeripheralSlot3].SetUsable(false);
        _myBoard[(int)GameBoard.Slot.PeripheralSlot4].SetUsable(_isExtendedTurn);
        _myBoard[(int)GameBoard.Slot.PeripheralSlot5].SetUsable(false);

        foreach (CardSlot slot in _opponentBoard) slot.SetUsable(false);

        if (UpdateCardSlotUsability()) _stateManager.FinishState();
    }

    public override void OnUpdateState()
    {
        Debug.Log($"Updating Player {(_isP1Turn ? 1 : 2)} Turn...");

        if (UpdateCardSlotUsability()) _stateManager.FinishState();
    }

    /// <summary>
    /// Updates the usable flag in all card slots based on what is filled.
    /// </summary>
    /// <returns>Whether all slots are filled.</returns>
    private bool UpdateCardSlotUsability()
    {
        bool allFilled = true;
        int startNum = _isExtendedTurn ? 4 : 0;
        int endNum = _isExtendedTurn ? 5 : 3;

        for (int i = startNum; i <= endNum; i++)
        {
            if (!_myBoard[i].HasCard)
            {
                _myBoard[i].SetUsable(true);
                allFilled = false;
                break;
            }
            else
            {
                _myBoard[i].SetUsable(false);
            }
        }

        return allFilled;
    }
}
