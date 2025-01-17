

public class GSInterrupt : GameState
{
    bool[,] _preInterruptSlotStates;

    public override void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        _stateManager = stateManager;
        _gameBoard = board;
        _preInterruptSlotStates = new bool[2,6];

        for (int i = 0; i < _gameBoard.Player1Board.Length; i++)
        {
            CardSlot p1Slot = _gameBoard.Player1Board[i];
            CardSlot p2Slot = _gameBoard.Player2Board[i];

            _preInterruptSlotStates[0,i] = p1Slot.IsUsable;
            _preInterruptSlotStates[1,i] = p2Slot.IsUsable;

            p1Slot.SetUsable(false);
            p2Slot.SetUsable(false);
        }

        _gameBoard.UtilitySlot.SetUsable(false);
    }

    public override void OnUpdateState()
    {
        for (int i = 0; i < _preInterruptSlotStates.GetLength(1); i++)
        {
            _gameBoard.Player1Board[i].SetUsable(_preInterruptSlotStates[0,i]);
            _gameBoard.Player2Board[i].SetUsable(_preInterruptSlotStates[1,i]);
        }

        _gameBoard.UtilitySlot.SetUsable(true);
        _stateManager.FinishState();
    }
}
