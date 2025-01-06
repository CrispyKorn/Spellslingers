using System.Linq;
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
    public CardSlot UtilitySlot { get => _utilitySlot; }

    [SerializeField] private CardSlot[] _player1Board = new CardSlot[6];
    [SerializeField] private CardSlot[] _player2Board = new CardSlot[6];
    [SerializeField] private CardSlot _utilitySlot;

    public bool IsSlotOnPlayer1Board(CardSlot slot)
    {
        return _player1Board.Contains(slot);
    }
}
