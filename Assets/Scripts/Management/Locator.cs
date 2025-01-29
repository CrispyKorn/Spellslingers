using UnityEngine;

public class Locator : MonoBehaviour
{
    public static Locator Instance;

    public DebugMenu DebugMenu { get => _debugMenu; }
    public InputManager InputManager { get => _inputManager; }
    public PlayManager PlayManager { get => _playManager; }
    public GameStateManager GameStateManager { get => _gameStateManager; }
    public PlayerManager PlayerManager { get => _playerManager; }
    public CardManager CardManager { get => _cardManager; }
    public UtilityManager UtilityManager { get => _utilityManager; }
    public GameBoard GameBoard { get => _gameBoard; }
    public RelayManager RelayManager { get => _relayManager; }
    public UIManager UIManager { get => _uiManager; }
    public DamageIndicatorManager DamageIndicatorManager { get => _damageIndicatorManager; }

    private DebugMenu _debugMenu;
    private InputManager _inputManager;
    private PlayManager _playManager;
    private GameStateManager _gameStateManager;
    private PlayerManager _playerManager;
    private CardManager _cardManager;
    private UtilityManager _utilityManager;
    private GameBoard _gameBoard;
    private RelayManager _relayManager;
    private UIManager _uiManager;
    private DamageIndicatorManager _damageIndicatorManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        DontDestroyOnLoad(this);
    }

    public void RegisterInstance(DebugMenu instance) { if (instance != null) _debugMenu = instance; }

    public void RegisterInstance(InputManager instance) { if (instance != null) _inputManager = instance; }

    public void RegisterInstance(PlayManager instance) { if (instance != null) _playManager = instance; }

    public void RegisterInstance(GameStateManager instance) { if (instance != null) _gameStateManager = instance; }

    public void RegisterInstance(PlayerManager instance) { if (instance != null) _playerManager = instance; }

    public void RegisterInstance(CardManager instance) { if (instance != null) _cardManager = instance; }

    public void RegisterInstance(UtilityManager instance) { if (instance != null) _utilityManager = instance; }

    public void RegisterInstance(GameBoard instance) { if (instance != null) _gameBoard = instance; }

    public void RegisterInstance(RelayManager instance) { if (instance != null) _relayManager = instance; }

    public void RegisterInstance(UIManager instance) { if (instance != null) _uiManager = instance; }
    public void RegisterInstance(DamageIndicatorManager instance) { if (instance != null) _damageIndicatorManager = instance; }
}
