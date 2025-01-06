using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GSBattle : GameState
{
    /// <summary>
    /// Fires when damage is dealt to a player. (player 1 is attacking, damage amount)
    /// </summary>
    public event Action<bool, int> OnDamageDealt;

    public override void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        _stateManager = stateManager;
        _gameBoard = board;

        foreach (CardSlot slot in _gameBoard.Player1Board)
        {
            slot.IsUsable = false;
        }

        foreach (CardSlot slot in _gameBoard.Player2Board)
        {
            slot.IsUsable = false;
        }

        // Calculate Damage
        CombinedCardValues p1Values = GetPlayerValues(_gameBoard.Player1Board);
        CombinedCardValues p2Values = GetPlayerValues(_gameBoard.Player2Board);

        // Compare Values
        var p1Atk = 0;
        var p2Atk = 0;

        if (stateManager.P1Attacking)
        {
            CalculateDmg(ref p1Atk, ref p2Atk,  p1Values.FireValues,  p2Values.FireValues);
            CalculateDmg(ref p1Atk, ref p2Atk,  p1Values.WaterValues,  p2Values.WaterValues);
            CalculateDmg(ref p1Atk, ref p2Atk,  p1Values.ElectricityValues,  p2Values.ElectricityValues);
        }
        else
        {
            CalculateDmg(ref p2Atk, ref p1Atk,  p2Values.FireValues,  p1Values.FireValues);
            CalculateDmg(ref p2Atk, ref p1Atk,  p2Values.WaterValues,  p1Values.WaterValues);
            CalculateDmg(ref p2Atk, ref p1Atk,  p2Values.ElectricityValues,  p1Values.ElectricityValues);
        }

        Debug.Log("Player 1 dealt " + p1Atk + " damage!");
        Debug.Log("Player 2 dealt " + p2Atk + " damage!");

        OnDamageDealt?.Invoke(true, p1Atk);
        OnDamageDealt?.Invoke(false, p2Atk);

        // Reset
        ResetBoard(_gameBoard.Player1Board);
        ResetBoard(_gameBoard.Player2Board);

        _stateManager.FinishState();
    }

    /// <summary>
    /// Resets the given game board, sending cards back to decks and performing cleanup.
    /// </summary>
    /// <param name="_board">The board (set of card slots) to reset.</param>
    private void ResetBoard(CardSlot[] _board)
    {
        foreach (CardSlot slot in _board)
        {
            if (slot.HasCard)
            {
                GameObject card = slot.TakeCard();
                _stateManager.CardManager.AddToDeck(card.GetComponent<PlayCard>().CardData);
                card.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    /// <summary>
    /// Gets the values of all played cards combined for the given player board.
    /// </summary>
    /// <param name="_playerBoard">The board for which to get the values.</param>
    /// <returns>The combined values of the played cards.</returns>
    private CombinedCardValues GetPlayerValues(CardSlot[] _playerBoard)
    {
        var coreCard = (CoreCard)_playerBoard[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>().CardData;
        var cardSlots = new Card[5];
        GameObject peripheralCard;

        for (int i = 1; i <= 5; i++)
        {
            peripheralCard = _playerBoard[i].Card;
            if (peripheralCard != null) cardSlots[i-1] = (Card)peripheralCard.GetComponent<PlayCard>().CardData;
        }

        var cards = new List<Card>();
        foreach (Card card in cardSlots)
        {
            if (card != null) cards.Add(card);
        }

        return coreCard.CalculateFinalValues(cards.ToArray());
    }

    /// <summary>
    /// Calculates the total damage absorbed by defences in an exchange. Use to calculate just one element at a time.
    /// </summary>
    /// <param name="attackerAtk">Attacking player's final attack value.</param>
    /// <param name="defenderAtk">Defending player's final attack value (from deflects).</param>
    /// <param name="attackerValues">The values used to calculate the attackers damage.</param>
    /// <param name="defenderValues">The values used to calculate the defenders defence.</param>
    private void CalculateDmg(ref int attackerAtk, ref int defenderAtk, CardValues attackerValues, CardValues defenderValues)
    {
        // Special v Special
        while (attackerValues.Special > 0 && defenderValues.Special > 0)
        {
            attackerValues.Special--;
            defenderValues.Special--;
            defenderValues.Power--;
        }

        // Special v Power
        while (attackerValues.Special > 0 && defenderValues.Power > 0)
        {
            attackerValues.Special--;
            defenderValues.Power--;
        }

        // Power v Special
        while (attackerValues.Power > 0 && defenderValues.Special > 0)
        {
            attackerValues.Power--;
            defenderValues.Special--;
            defenderValues.Power--;
            defenderAtk++;
        }

        // Power v Power
        int finalValue = attackerValues.Power - defenderValues.Power;
        if (finalValue < 0) finalValue = 0;
        attackerAtk += finalValue;
    }

    public override void OnUpdateState()
    {
        
    }
}
