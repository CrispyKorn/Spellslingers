

public abstract class GameState
{
    protected GameStateManager _stateManager;
    protected GameBoard _gameBoard;

    /// <summary>
    /// Called when entering the state.
    /// </summary>
    /// <param name="stateManager">The active gamestate manager object.</param>
    /// <param name="board">The active game board object.</param>
    public abstract void OnEnterState(GameStateManager stateManager, GameBoard board);

    /// <summary>
    /// Called whenever the game state is updated, such as when a card is placed.
    /// </summary>
    public abstract void OnUpdateState();
}
