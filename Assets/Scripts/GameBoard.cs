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

    public CardSlot[] player1Board = new CardSlot[6];
    public CardSlot[] player2Board = new CardSlot[6];
}
