using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button BtnPass { get => _btnPass; }

    [SerializeField] private TextMeshProUGUI _txtP1Health;
    [SerializeField] private TextMeshProUGUI _txtP2Health;
    [SerializeField] private TextMeshProUGUI _txtGameOver;
    [SerializeField] private TextMeshProUGUI _txtTurn;
    [SerializeField] private GameObject _pnlGameOver;
    [SerializeField] private Button _btnPass;

    /// <summary>
    /// Sets the visibility of the 'Pass' button.
    /// </summary>
    /// <param name="visible"></param>
    private void ChangePassBtnVisibility(bool visible)
    {
        _btnPass.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Updates the game UI
    /// </summary>
    /// <param name="player1Hp">The current health of player 1 (host).</param>
    /// <param name="player2Hp">The current health of player 2 (client).</param>
    /// <param name="currentState">The current value of the game state.</param>
    public void UpdateUI(int player1Hp, int player2Hp, int currentState, bool isHost)
    {
        _txtP1Health.text = "HP: " + player1Hp;
        _txtP2Health.text = "HP: " + player2Hp;

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

        // Update pass button visibility
        bool enabledForPlayer1 = currentState == (int)GameStateManager.GameStateIndex.Player1Turn || currentState == (int)GameStateManager.GameStateIndex.Player1ExtendedTurn;
        bool enabledForPlayer2 = currentState == (int)GameStateManager.GameStateIndex.Player2Turn || currentState == (int)GameStateManager.GameStateIndex.Player2ExtendedTurn;
        bool activeForMe = isHost ? enabledForPlayer1 : enabledForPlayer2;

        ChangePassBtnVisibility(activeForMe);
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
    public void SwapPlayerHealth()
    {
        TextMeshProUGUI temp = _txtP1Health;
        _txtP1Health = _txtP2Health;
        _txtP2Health = temp;
    }
}
