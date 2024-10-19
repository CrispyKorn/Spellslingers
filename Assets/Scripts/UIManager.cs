using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI txtP1Health, txtP2Health, txtGameOver, txtTurn;
    [SerializeField] GameObject pnlGameOver;
    [SerializeField] Button btnPass;

    public Button BtnPass { get { return btnPass; } }

    public void UpdateUI(int player1Hp, int player2Hp, int currentState)
    {
        txtP1Health.text = "HP: " + player1Hp;
        txtP2Health.text = "HP: " + player2Hp;

        string turnText;
        switch (currentState)
        {
            case 0: turnText = "P1 Turn"; break;
            case 1: turnText = "P2 Turn"; break;
            case 2: turnText = "Interrupt"; break;
            case 3: turnText = "Battle"; break;
            default: turnText = "Error"; break;
        }
        txtTurn.text = turnText;
    }

    public void GameOverUI(bool player1Won)
    {
        txtGameOver.text = "Game Over!\n\n";
        txtGameOver.text += player1Won ? "Player 1 Wins!" : "Player 2 Wins!";
        pnlGameOver.SetActive(true);
    }

    public void ChangePassBtnVisibility(bool visible)
    {
        btnPass.gameObject.SetActive(visible);
    }
}
