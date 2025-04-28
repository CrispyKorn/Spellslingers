using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GSBattle : GameState
{
    /// <summary>
    /// Fires when damage is dealt to a player. (player 1 is attacking, damage amount)
    /// </summary>
    public event Action<bool, int> OnDamageDealt;

    private DamageIndicatorManager _damageIndicatorManager;

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
        _damageIndicatorManager = Locator.Instance.DamageIndicatorManager;

        _damageIndicatorManager.ClearIndicatorsRpc();

        // Update P1 indicators
        _damageIndicatorManager.SetIndicatorsRpc(true, true, Card.CardElement.Fire, p1Values.OffenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(true, true, Card.CardElement.Water, p1Values.OffenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(true, true, Card.CardElement.Electricity, p1Values.OffenceValues.ElectricityValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(true, false, Card.CardElement.Fire, p1Values.DefenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(true, false, Card.CardElement.Water, p1Values.DefenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(true, false, Card.CardElement.Electricity, p1Values.DefenceValues.ElectricityValues);
        await Awaitable.NextFrameAsync();

        // Update P2 indicators
        _damageIndicatorManager.SetIndicatorsRpc(false, true, Card.CardElement.Fire, p2Values.OffenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(false, true, Card.CardElement.Water, p2Values.OffenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(false, true, Card.CardElement.Electricity, p2Values.OffenceValues.ElectricityValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(false, false, Card.CardElement.Fire, p2Values.DefenceValues.FireValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(false, false, Card.CardElement.Water, p2Values.DefenceValues.WaterValues);
        await Awaitable.NextFrameAsync();
        _damageIndicatorManager.SetIndicatorsRpc(false, false, Card.CardElement.Electricity, p2Values.DefenceValues.ElectricityValues);

        await Awaitable.WaitForSecondsAsync(3f);

        // P1 Attacking
        await PlayBattleAnimation(true, Card.CardElement.Fire);
        await PlayBattleAnimation(false, Card.CardElement.Fire);
        await PlayBattleAnimation(true, Card.CardElement.Electricity);
        await PlayBattleAnimation(false, Card.CardElement.Electricity);
        await PlayBattleAnimation(true, Card.CardElement.Water);
        await PlayBattleAnimation(false, Card.CardElement.Water);

        await Awaitable.WaitForSecondsAsync(5f);

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
    /// <param name="defenderValues">The values used to calculate the defenders damage.</param>
    private void CalculateDamage(ref int attackerAtk, ref int defenderAtk, CardValues attackerValuesRef, CardValues defenderValuesRef)
    {
        // Make copies to preserve values
        CardValues attackerValues = new(attackerValuesRef.Power, attackerValuesRef.Special);
        CardValues defenderValues = new(defenderValuesRef.Power, attackerValuesRef.Special);

        // Special v Special
        while (attackerValues.Special > 0 && defenderValues.Special > 0)
        {
            attackerValues.Special--;
            defenderValues.Special--;
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
            defenderAtk++;
        }

        // Power v Power
        int finalValue = attackerValues.Power - defenderValues.Power;
        
        if (finalValue < 0) finalValue = 0;
        attackerAtk += finalValue;
    }

    /// <summary>
    /// Plays a set of indicator animations for the specified set.
    /// </summary>
    /// <param name="p1Attacking">Whether player 1 (host) is attacking.</param>
    /// <param name="element">The element of the indicator set</param>
    /// <returns>When all animations in the set are complete.</returns>
    private async Task PlayBattleAnimation(bool p1Attacking, Card.CardElement element)
    {
        await PlayStrikeAnimation(p1Attacking, element, true, true); // Special v Special
        await PlayStrikeAnimation(p1Attacking, element, true, false); // Special v Power
        await PlayStrikeAnimation(p1Attacking, element, false, true); // Power v Special
        await PlayStrikeAnimation(p1Attacking, element, false, false); // Power vs Power
    }

    /// <summary>
    /// Plays an indivial indicator pair animation.
    /// </summary>
    /// <param name="p1Attacking">Whether player 1 (host) is attacking.</param>
    /// <param name="element">The element of the indicators.</param>
    /// <param name="attackingSpecial">Whether to use the special attack indicator.</param>
    /// <param name="defendingSpecial">Whether to use the special defence indicator.</param>
    /// <returns>When the animation is complete.</returns>
    private async Task PlayStrikeAnimation(bool p1Attacking, Card.CardElement element, bool attackingSpecial, bool defendingSpecial)
    {
        int attackValue = _damageIndicatorManager.GetIndicatorValue(p1Attacking, element, true, attackingSpecial);
        int defenceValue = _damageIndicatorManager.GetIndicatorValue(!p1Attacking, element, false, defendingSpecial);
        bool attackEmpty = attackValue == 0;
        bool defenceEmpty = defenceValue == 0;
        int attackerDamage = 0;
        int defenderDamage = 0;

        if (attackEmpty) return;
        if (defendingSpecial && defenceEmpty) return;

        // Update indicators to match values of attack - defence and defence - attack
        if (defendingSpecial && !attackingSpecial) defenderDamage = Math.Min(defenceValue, attackValue);

        int prevAttackValue = attackValue;
        attackValue = Math.Max(0, attackValue - defenceValue);
        defenceValue = Math.Max(0, defenceValue - prevAttackValue);

        if (!defendingSpecial && !attackingSpecial) attackerDamage = attackValue;

        if (!defenceEmpty) _ = _damageIndicatorManager.AnimateIndicator(!p1Attacking, element, false, defendingSpecial, defenceValue, DealDamage, defenderDamage);
        else attackValue = 0; // Use up all attack value on dealing damage

        await _damageIndicatorManager.AnimateIndicator(p1Attacking, element, true, attackingSpecial, attackValue, DealDamage, attackerDamage);
    }

    private void DealDamage(bool toP2, int amount)
    {
        OnDamageDealt?.Invoke(toP2, amount);
    }

    public override void OnUpdateState()
    {
        
    }
}
