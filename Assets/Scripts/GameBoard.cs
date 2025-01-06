using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public enum Slot
    {
        CoreSlot,
        PeripheralSlot1,
        PeripheralSlot2,
        PeripheralSlot3,
        PeripheralSlot4,
        PeripheralSlot5,
    }

    public CardSlot[] Player1Board { get => _player1Board; }
    public CardSlot[] Player2Board { get => _player2Board; }

    [SerializeField] private CardSlot[] _player1Board = new CardSlot[6];
    [SerializeField] private CardSlot[] _player2Board = new CardSlot[6];
}
