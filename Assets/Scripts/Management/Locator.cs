using UnityEngine;

public class Locator : MonoBehaviour
{
    public static Locator Instance;

    public DebugMenu DebugMenu { get => _debugMenu; }
    public InputManager InputManager { get => _inputManager; }
    public PlayManager PlayManager { get => _playManager; }
    public PlayerManager PlayerManager { get => _playerManager; }
    public CardManager CardManager { get => _cardManager; }
    public RelayManager RelayManager { get => _relayManager; }
    public UIManager UIManager { get => _uiManager; }

    private DebugMenu _debugMenu;
    private InputManager _inputManager;
    private PlayManager _playManager;
    private PlayerManager _playerManager;
    private CardManager _cardManager;
    private RelayManager _relayManager;
    private UIManager _uiManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        DontDestroyOnLoad(this);
    }

    public void RegisterInstance(DebugMenu instance)
    {
        if (instance != null) _debugMenu = instance;
    }

    public void RegisterInstance(InputManager instance)
    {
        if (instance != null) _inputManager = instance;
    }

    public void RegisterInstance(PlayManager instance)
    {
        if (instance != null) _playManager = instance;
    }

    public void RegisterInstance(PlayerManager instance)
    {
        if (instance != null) _playerManager = instance;
    }

    public void RegisterInstance(CardManager instance)
    {
        if (instance != null) _cardManager = instance;
    }

    public void RegisterInstance(RelayManager instance)
    {
        if (instance != null) _relayManager = instance;
    }

    public void RegisterInstance(UIManager instance)
    {
        if (instance != null) _uiManager = instance;
    }
}
