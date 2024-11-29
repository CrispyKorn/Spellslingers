using UnityEngine;

public class Locator : MonoBehaviour
{
    public static Locator Instance;

    public DebugMenu DebugMenu { get => _debugMenu; }
    public InputManager InputManager { get => _inputManager; }
    public PlayManager PlayManager { get => _playManager; }
    public CardManager CardManager { get => _cardManager; }
    public RelayManager RelayManager { get => _relayManager; }

    private DebugMenu _debugMenu;
    private InputManager _inputManager;
    private PlayManager _playManager;
    private CardManager _cardManager;
    private RelayManager _relayManager;

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

    public void RegisterInstance(CardManager instance)
    {
        if (instance != null) _cardManager = instance;
    }

    public void RegisterInstance(RelayManager instance)
    {
        if (instance != null) _relayManager = instance;
    }
}
