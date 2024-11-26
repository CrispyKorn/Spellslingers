using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button BtnPass { get => _btnPass; }

    [SerializeField] TextMeshProUGUI _txtP1Health;
    [SerializeField] TextMeshProUGUI _txtP2Health;
    [SerializeField] TextMeshProUGUI _txtGameOver;
    [SerializeField] TextMeshProUGUI _txtTurn;
    [SerializeField] GameObject _pnlGameOver;
    [SerializeField] Button _btnPass;

    public void UpdateUI(int player1Hp, int player2Hp, int currentState)
    {
        _txtP1Health.text = "HP: " + player1Hp;
        _txtP2Health.text = "HP: " + player2Hp;

        string turnText;
        switch (currentState)
        {
            case 0: turnText = "P1 Turn"; break;
            case 1: turnText = "P2 Turn"; break;
            case 2: turnText = "Interrupt"; break;
            case 3: turnText = "Battle"; break;
            default: turnText = "Error"; break;
        }
        _txtTurn.text = turnText;
    }

    public void GameOverUI(bool player1Won)
    {
        _txtGameOver.text = "Game Over!\n\n";
        _txtGameOver.text += player1Won ? "Player 1 Wins!" : "Player 2 Wins!";
        _pnlGameOver.SetActive(true);
    }

    public void ChangePassBtnVisibility(bool visible)
    {
        _btnPass.gameObject.SetActive(visible);
    }
}
