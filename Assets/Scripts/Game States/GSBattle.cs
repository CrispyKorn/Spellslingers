using System;
using System.Collections.Generic;
using UnityEngine;

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

        for (int i = 0; i < _gameBoard.Player1Board.Length; i++)
        {
            _gameBoard.Player1Board[i].SetUsable(false);
            _gameBoard.Player2Board[i].SetUsable(false);
        }

        _gameBoard.UtilitySlot.SetUsable(false);

        // Calculate Damage
        CardValueSet p1Values = GetPlayerValues(_gameBoard.Player1Board);
        CardValueSet p2Values = GetPlayerValues(_gameBoard.Player2Board);

        // Compare Values
        var p1Atk = 0;
        var p2Atk = 0;

        CalculateDmg(ref p1Atk, ref p2Atk,  p1Values.OffenceValues.FireValues,  p2Values.DefenceValues.FireValues);
        CalculateDmg(ref p1Atk, ref p2Atk,  p1Values.OffenceValues.WaterValues,  p2Values.DefenceValues.WaterValues);
        CalculateDmg(ref p1Atk, ref p2Atk,  p1Values.OffenceValues.ElectricityValues,  p2Values.DefenceValues.ElectricityValues);
        CalculateDmg(ref p2Atk, ref p1Atk,  p2Values.OffenceValues.FireValues,  p1Values.DefenceValues.FireValues);
        CalculateDmg(ref p2Atk, ref p1Atk,  p2Values.OffenceValues.WaterValues,  p1Values.DefenceValues.WaterValues);
        CalculateDmg(ref p2Atk, ref p1Atk,  p2Values.OffenceValues.ElectricityValues,  p1Values.DefenceValues.ElectricityValues);

        Debug.Log($"Player 1 dealt {p1Atk} damage!");
        Debug.Log($"Player 2 dealt {p2Atk} damage!");

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
                _stateManager.CardManager.DiscardCard(card.GetComponent<PlayCard>());
            }
        }
    }

    /// <summary>
    /// Gets the values of all played cards combined for the given player board.
    /// </summary>
    /// <param name="_playerBoard">The board for which to get the values.</param>
    /// <returns>The combined values of the played cards.</returns>
    private CardValueSet GetPlayerValues(CardSlot[] _playerBoard)
    {
        var coreCard = (CoreCard)_playerBoard[(int)GameBoard.Slot.CoreSlot].Card.GetComponent<PlayCard>().CardData;
        var peripheralCards = new List<Card>();

        for (int i = 1; i <= 5; i++)
        {
            GameObject peripheralCard = _playerBoard[i].Card;
            if (peripheralCard != null)
            {
                ICard cardData = peripheralCard.GetComponent<PlayCard>().CardData;
                if (cardData.Type != ICard.CardType.Utility) peripheralCards.Add((Card)cardData);
            }
        }

        return coreCard.CalculateFinalValues(peripheralCards);
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
