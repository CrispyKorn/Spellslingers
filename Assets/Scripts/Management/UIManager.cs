using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIManager : NetworkBehaviour
{
    public Button BtnPass { get => _btnPass; }
    public Image ZoomCard { get => _zoomCard; }
    public ElementSelectionManager ElementSelectionManager { get => _elementSelectionManager; }
    public PeripheralSelectionManager PeripheralSelectionManager { get => _peripheralSelectionManager; }

    [SerializeField] private TextMeshProUGUI _txtP1Health;
    [SerializeField] private TextMeshProUGUI _txtP2Health;
    [SerializeField] private TextMeshProUGUI _txtGameOver;
    [SerializeField] private TextMeshProUGUI _txtTurn;
    [SerializeField] private TextMeshProUGUI _txtUtilitySlot;
    [SerializeField] private GameObject _pnlGameOver;
    [SerializeField] private Button _btnPass;
    [SerializeField] private Image _zoomCard;
    [SerializeField] private ElementSelectionManager _elementSelectionManager;
    [SerializeField] private PeripheralSelectionManager _peripheralSelectionManager;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
    }

    /// <summary>
    /// Updates the UI for player health.
    /// </summary>
    /// <param name="player1Hp">The current health of player 1 (host).</param>
    /// <param name="player2Hp">The current health of player 2 (client).</param>
    [Rpc(SendTo.Everyone)]
    public void UpdateUIHealthRpc(int player1Hp, int player2Hp)
    {
        _txtP1Health.text = "HP: " + player1Hp;
        _txtP2Health.text = "HP: " + player2Hp;
    }
    /// <summary>
    /// Updates the UI for game state.
    /// </summary>
    /// <param name="currentState">The current value of the game state.</param>
    [Rpc(SendTo.Everyone)]
    public void UpdateUIStateRpc(int currentState)
    {
        string turnText;

        switch (currentState)
        {
            case 0: turnText = "Interrupt"; break;
            case 1: turnText = "P1 Turn"; break;
            case 2: turnText = "P2 Turn"; break;
            case 3: turnText = "P1 Extended Turn"; break;
            case 4: turnText = "P2 Extended Turn"; break;
            case 5: turnText = "Battle"; break;
            default: turnText = "Error"; break;
        }

        _txtTurn.text = turnText;
    }

    /// <summary>
    /// Updates the UI for the pass button.
    /// </summary>
    /// <param name="activeForP1">Whether the pass button is active for player 1 (host).</param>
    /// <param name="activeForP2">Whether the pass button is active for player 2 (client).</param>
    [Rpc(SendTo.Everyone)]
    public void UpdateUIPassBtnRpc(bool activeForP1, bool activeForP2)
    {
        _btnPass.gameObject.SetActive(IsHost ? activeForP1 : activeForP2);
    }

    /// <summary>
    /// Sets and enables the 'Game Over' screen UI.
    /// </summary>
    /// <param name="player1Won">Whether player 1 (host) was the winner.</param>
    public void GameOverUI(bool player1Won)
    {
        _txtGameOver.text = "Game Over!\n\n";
        _txtGameOver.text += player1Won ? "Player 1 Wins!" : "Player 2 Wins!";
        _pnlGameOver.SetActive(true);
    }

    /// <summary>
    /// Visually swaps the health for each player (for player 2 since their camera is flipped).
    /// </summary>
    public void SwapUIElements()
    {
        // Health
        TextMeshProUGUI temp = _txtP1Health;
        _txtP1Health = _txtP2Health;
        _txtP2Health = temp;

        // Utility slot text
        _txtUtilitySlot.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180);

        // Pass button
        var passBtnTransform = _btnPass.GetComponent<RectTransform>();
        Vector3 passBtnPos = passBtnTransform.localPosition;
        passBtnTransform.localPosition = new Vector3(-passBtnPos.x, passBtnPos.y, passBtnPos.z);

        // Zoom Card
        var zoomCardTransform = _zoomCard.GetComponent<RectTransform>();
        Vector3 zoomCardPos = zoomCardTransform.localPosition;
        zoomCardTransform.localPosition = new Vector3(-zoomCardPos.x, zoomCardPos.y, zoomCardPos.z);
    }

    public void SetElementSelectionActive(bool active, ulong targetClientId)
    {
        _elementSelectionManager.SetButtonsActiveRpc(active, RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
    }

    public void SetPeripheralSelectionActive(bool active, ulong targetClientId)
    {
        _peripheralSelectionManager.SetButtonsActiveRpc(active, RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
    }
}
