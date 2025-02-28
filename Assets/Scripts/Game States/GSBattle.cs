using System;
using System.Collections.Generic;
using UnityEngine;

public class GSBattle : GameState
{
    /// <summary>
    /// Fires when damage is dealt to a player. (player 1 is attacking, damage amount)
    /// </summary>
    public event Action<bool, int> OnDamageDealt;

    public override async void OnEnterState(GameStateManager stateManager, GameBoard board)
    {
        _stateManager = stateManager;
        _gameBoard = board;

        // Cleanup
        for (int i = 0; i < _gameBoard.Player1Board.Length; i++)
        {
            _gameBoard.Player1Board[i].SetUsable(false);
            _gameBoard.Player2Board[i].SetUsable(false);
        }

        _gameBoard.UtilitySlot.SetUsable(false);

        _stateManager.CardManager.SetCardHighlights(false, true);
        _stateManager.CardManager.SetCardHighlights(false, false);

        // Calculate damage
        CardValueSet p1Values = GetPlayerValues(_gameBoard.Player1Board);
        CardValueSet p2Values = GetPlayerValues(_gameBoard.Player2Board);

        // Compare values
        var p1Atk = 0;
        var p2Atk = 0;

        CalculateDamage(ref p1Atk, ref p2Atk,  p1Values.OffenceValues.FireValues,  p2Values.DefenceValues.FireValues);
        CalculateDamage(ref p1Atk, ref p2Atk,  p1Values.OffenceValues.WaterValues,  p2Values.DefenceValues.WaterValues);
        CalculateDamage(ref p1Atk, ref p2Atk,  p1Values.OffenceValues.ElectricityValues,  p2Values.DefenceValues.ElectricityValues);
        CalculateDamage(ref p2Atk, ref p1Atk,  p2Values.OffenceValues.FireValues,  p1Values.DefenceValues.FireValues);
        CalculateDamage(ref p2Atk, ref p1Atk,  p2Values.OffenceValues.WaterValues,  p1Values.DefenceValues.WaterValues);
        CalculateDamage(ref p2Atk, ref p1Atk,  p2Values.OffenceValues.ElectricityValues,  p1Values.DefenceValues.ElectricityValues);

        Debug.Log($"Player 1 dealt {p1Atk} damage!");
        Debug.Log($"Player 2 dealt {p2Atk} damage!");

        // Update indicators
        DamageIndicatorManager damageIndicatorManager = Locator.Instance.DamageIndicatorManager;

        damageIndicatorManager.ClearIndicatorsRpc();

        // Update P1 indicators
        damageIndicatorManager.SetIndicatorsRpc(true, true, Card.CardElement.Fire, p1Values.OffenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(true, true, Card.CardElement.Water, p1Values.OffenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(true, true, Card.CardElement.Electricity, p1Values.OffenceValues.ElectricityValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(true, false, Card.CardElement.Fire, p1Values.DefenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(true, false, Card.CardElement.Water, p1Values.DefenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(true, false, Card.CardElement.Electricity, p1Values.DefenceValues.ElectricityValues);
        await Awaitable.NextFrameAsync();

        // Update P2 indicators
        damageIndicatorManager.SetIndicatorsRpc(false, true, Card.CardElement.Fire, p2Values.OffenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(false, true, Card.CardElement.Water, p2Values.OffenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(false, true, Card.CardElement.Electricity, p2Values.OffenceValues.ElectricityValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(false, false, Card.CardElement.Fire, p2Values.DefenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(false, false, Card.CardElement.Water, p2Values.DefenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        damageIndicatorManager.SetIndicatorsRpc(false, false, Card.CardElement.Electricity, p2Values.DefenceValues.ElectricityValues);

        await Awaitable.WaitForSecondsAsync(5f);

        // Apply damage
        OnDamageDealt?.Invoke(true, p1Atk);
        OnDamageDealt?.Invoke(false, p2Atk);

        // Reset
        ResetBoard(_gameBoard.Player1Board);
        ResetBoard(_gameBoard.Player2Board);
        Locator.Instance.DamageIndicatorManager.ClearIndicatorsRpc();

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
    private void CalculateDamage(ref int attackerAtk, ref int defenderAtk, CardValues attackerValues, CardValues defenderValues)
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
