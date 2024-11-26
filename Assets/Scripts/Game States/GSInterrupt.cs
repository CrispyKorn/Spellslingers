

public class GSInterrupt : GameState
{
    public override void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        _stateManager = stateManager;
        _gameBoard = board;

        foreach (CardSlot slot in _gameBoard.player1Board) slot.IsUsable = false;
        foreach (CardSlot slot in _gameBoard.player2Board) slot.IsUsable = false;
    }

    public override void OnUpdateState()
    {
        
    }
}
