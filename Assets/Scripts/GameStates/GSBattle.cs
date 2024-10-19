using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GSBattle : GameState
{
    public event System.Action<bool, int> OnDamageDealt;

    public override void OnEnterState(GameStateManager _stateManager, GameBoard _board)
    {
        stateManager = _stateManager;
        gameBoard = _board;

        foreach (CardSlot slot in gameBoard.player1Board)
        {
            slot.useable.Value = false;
        }

        foreach (CardSlot slot in gameBoard.player2Board)
        {
            slot.useable.Value = false;
        }

        //Calculate Damage
        CoreCard.CombinedCardValues p1Values = GetPlayerValues(gameBoard.player1Board);
        CoreCard.CombinedCardValues p2Values = GetPlayerValues(gameBoard.player2Board);

        //Compare Values
        int p1Atk = 0;
        int p2Atk = 0;

        if (_stateManager.P1Attacking)
        {
            CalculateDmg(ref p1Atk, ref p2Atk, ref p1Values.fireValues, ref p2Values.fireValues);
            CalculateDmg(ref p1Atk, ref p2Atk, ref p1Values.waterValues, ref p2Values.waterValues);
            CalculateDmg(ref p1Atk, ref p2Atk, ref p1Values.electricityValues, ref p2Values.electricityValues);
        }
        else
        {
            CalculateDmg(ref p2Atk, ref p1Atk, ref p2Values.fireValues, ref p1Values.fireValues);
            CalculateDmg(ref p2Atk, ref p1Atk, ref p2Values.waterValues, ref p1Values.waterValues);
            CalculateDmg(ref p2Atk, ref p1Atk, ref p2Values.electricityValues, ref p1Values.electricityValues);
        }

        Debug.Log("Player 1 dealt " + p1Atk + " damage!");
        Debug.Log("Player 2 dealt " + p2Atk + " damage!");

        OnDamageDealt?.Invoke(true, p1Atk);
        OnDamageDealt?.Invoke(false, p2Atk);

        //Reset
        ResetBoard(gameBoard.player1Board);
        ResetBoard(gameBoard.player2Board);
    }

    private void ResetBoard(CardSlot[] _board)
    {
        foreach (CardSlot slot in _board)
        {
            if (slot.HasCard)
            {
                GameObject card = slot.TakeCard();
                stateManager.CardManager.AddToDeck(card.GetComponent<PlayCard>().cardData);
                card.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private CoreCard.CombinedCardValues GetPlayerValues(CardSlot[] _playerBoard)
    {
        CoreCard coreCard = _playerBoard[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>().cardData as CoreCard;
        Card[] cardSlots = new Card[5];
        GameObject peripheralCard;

        for (int i = 1; i <= 5; i++)
        {
            peripheralCard = _playerBoard[i].Card;
            if (peripheralCard != null) cardSlots[i-1] = (Card)peripheralCard.GetComponent<PlayCard>().cardData;
        }

        List<Card> cards = new List<Card>();
        foreach (Card card in cardSlots)
        {
            if (card != null) cards.Add(card);
        }

        return coreCard.CalculateFinalValues(cards.ToArray());
    }

    private void CalculateDmg(ref int attackerAtk, ref int defenderAtk, ref Card.CardValues attackerValues, ref Card.CardValues defenderValues)
    {
        //Special v Special
        while (attackerValues.special > 0 && defenderValues.special > 0)
        {
            attackerValues.special--;
            defenderValues.special--;
            defenderValues.power--;
        }

        //Special v Power
        while (attackerValues.special > 0 && defenderValues.power > 0)
        {
            attackerValues.special--;
            defenderValues.power--;
        }

        //Power v Special
        while (attackerValues.power > 0 && defenderValues.special > 0)
        {
            attackerValues.power--;
            defenderValues.special--;
            defenderValues.power--;
            defenderAtk++;
        }

        //Power v Power
        int finalValue = attackerValues.power - defenderValues.power;
        if (finalValue < 0) finalValue = 0;
        attackerAtk += finalValue;
    }

    public override void OnUpdateState()
    {
        
    }
}
