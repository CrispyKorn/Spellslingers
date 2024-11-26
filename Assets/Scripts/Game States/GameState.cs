

public abstract class GameState
{
    protected GameStateManager _stateManager;
    protected GameBoard _gameBoard;

    public abstract void OnEnterState(GameStateManager stateManager, GameBoard board);
    public abstract void OnUpdateState();
}
