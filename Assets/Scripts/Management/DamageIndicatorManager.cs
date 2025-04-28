using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using TMPro;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DamageIndicatorManager : NetworkBehaviour
{    
    public IndicatorSet WaterIndicators { get => _waterIndicators; }
    public IndicatorSet FireIndicators { get => _fireIndicators; }
    public IndicatorSet ElectricityIndicators { get => _electricityIndicators; }

    [SerializeField] private IndicatorSet _waterIndicators;
    [SerializeField] private IndicatorSet _fireIndicators;
    [SerializeField] private IndicatorSet _electricityIndicators;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);

        _waterIndicators.Initialize();
        _fireIndicators.Initialize();
        _electricityIndicators.Initialize();
    }

    /// <summary>
    /// Finds the indicator that fits the given information.
    /// </summary>
    /// <param name="player1Indicator">Whether the indicator is for player 1.</param>
    /// <param name="element">The element of the indicator.</param>
    /// <returns>The matching indicator.</returns>
    private Indicator GetIndicator(bool player1Indicator, Card.CardElement element)
    {
        Indicator indicator = null;
        
        // Find relevant holder and colour
        switch (element)
        {
            case Card.CardElement.Water: 
            {
                indicator = player1Indicator ? _waterIndicators.P1Holder : _waterIndicators.P2Holder;
            }
            break;
            case Card.CardElement.Fire: 
            {
                indicator = player1Indicator ? _fireIndicators.P1Holder : _fireIndicators.P2Holder;
            }
            break;
            case Card.CardElement.Electricity: 
            {
                indicator = player1Indicator ? _electricityIndicators.P1Holder : _electricityIndicators.P2Holder;
            }
            break;
        }

        return indicator;
    }

    /// <summary>
    /// Sets the values of the specified indicator's number for both players.
    /// </summary>
    /// <param name="forPlayer1">Whether the indicator is for player 1.</param>
    /// <param name="isOffenceCard">Whether the played card is offence.</param>
    /// <param name="cardElement">The element of the played card.</param>
    /// <param name="cardValues">The values of the played card.</param>
    [Rpc(SendTo.Everyone)]
    public void SetIndicatorsRpc(bool forPlayer1, bool isOffenceCard, Card.CardElement cardElement, CardValues cardValues)
    {
        Indicator indicator = GetIndicator(forPlayer1, cardElement);
        
        // Set power and special indicators
        if (isOffenceCard)
        {
            indicator.AttackCounter += cardValues.Power;
            indicator.SpecialAttackCounter += cardValues.Special;
        }
        else
        {
            indicator.DefenceCounter += cardValues.Power;
            indicator.SpecialDefenceCounter += cardValues.Special;
        }

        indicator.UpdateText();
    }

    /// <summary>
    /// Resets the values to 0 for all indicators for all players.
    /// </summary>
    [Rpc(SendTo.Everyone)]
    public void ClearIndicatorsRpc()
    {
        _waterIndicators.ResetCounters();
        _fireIndicators.ResetCounters();
        _electricityIndicators.ResetCounters();
    }

    /// <summary>
    /// Manages animating the specified indicator for both host and client, awaiting for the end of the animation before returning.
    /// </summary>
    /// <param name="p1Indicator">Whether the indicator is for player 1.</param>
    /// <param name="element">The element of the indicator.</param>
    /// <param name="isAttack">Whether the indicator is an attack indicator.</param>
    /// <param name="isSpecial">Whether the indicator is a special indicator.</param>
    public async Task AnimateIndicator(bool p1Indicator, Card.CardElement element, bool isAttack, bool isSpecial, int indicatorValue, Action<bool, int> dealDamageCallback, int damageAmount)
    {
        TriggerClientAnimationRpc(p1Indicator, element, isAttack, isSpecial);
        await PlayIndicator(p1Indicator, element, isAttack, isSpecial, indicatorValue, dealDamageCallback, damageAmount);
    }

    /// <summary>
    /// Plays the indicator animation, waiting for the animation to finish before returning. Run by player 1 (host) only.
    /// </summary>
    /// <param name="p1Indicator">Whether the indicator is for player 1.</param>
    /// <param name="element">The element of the indicator.</param>
    /// <param name="isAttack">Whether the indicator is an attack indicator.</param>
    /// <param name="isSpecial">Whether the indicator is a special indicator.</param>
    /// <returns></returns>
    private async Task PlayIndicator(bool p1Indicator, Card.CardElement element, bool isAttack, bool isSpecial, int indicatorValue, Action<bool, int> dealDamageCallback, int damageAmount)
    {
        Indicator indicator = GetIndicator(p1Indicator, element);
        Animator animator = isAttack ? 
                                (isSpecial ? indicator.SpecialAttackCounterAnimator : indicator.AttackCounterAnimator) : 
                                (isSpecial ? indicator.SpecialDefenceCounterAnimator : indicator.DefenceCounterAnimator);

        animator.SetTrigger("Activate");
        await Awaitable.NextFrameAsync();

        bool passedMidpoint = false;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("IndicatorIdle"))
        {
            if (!passedMidpoint && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f) 
            {
                UpdateIndicatorTextRpc(p1Indicator, element, isAttack, isSpecial, indicatorValue);
                passedMidpoint = true;
                if (damageAmount > 0) dealDamageCallback.Invoke(p1Indicator, damageAmount);
            }

            await Awaitable.NextFrameAsync();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateIndicatorTextRpc(bool p1Indicator, Card.CardElement element, bool isAttack, bool isSpecial, int indicatorValue)
    {
        Indicator indicator = GetIndicator(p1Indicator, element);
        
        if (isAttack)
        {
            if (isSpecial) indicator.SpecialAttackCounter = indicatorValue;
            else indicator.AttackCounter = indicatorValue;
        }
        else
        {
            if (isSpecial) indicator.SpecialDefenceCounter = indicatorValue;
            else indicator.DefenceCounter = indicatorValue;
        }

        indicator.UpdateText();
    }

    /// <summary>
    /// Plays the indicator animation. Run by player 2 (client) only.
    /// </summary>
    /// <param name="p1Indicator">Whether the indicator is for player 1.</param>
    /// <param name="element">The element of the indicator.</param>
    /// <param name="isAttack">Whether the indicator is an attack indicator.</param>
    /// <param name="isSpecial">Whether the indicator is a special indicator.</param>
    [Rpc(SendTo.NotServer)]
    private void TriggerClientAnimationRpc(bool p1Indicator, Card.CardElement element, bool isAttack, bool isSpecial)
    {
        Indicator indicator = GetIndicator(p1Indicator, element);
        Animator animator = isAttack ? 
                                (isSpecial ? indicator.SpecialAttackCounterAnimator : indicator.AttackCounterAnimator) : 
                                (isSpecial ? indicator.SpecialDefenceCounterAnimator : indicator.DefenceCounterAnimator);

        animator.SetTrigger("Activate");
    }

    public int GetIndicatorValue(bool p1Attacking, Card.CardElement element, bool isAttack, bool isSpecial)
    {
        Indicator indicator = GetIndicator(p1Attacking, element);

        int value = isAttack ? 
                    (isSpecial ? indicator.SpecialAttackCounter : indicator.AttackCounter) : 
                    (isSpecial ? indicator.SpecialDefenceCounter : indicator.DefenceCounter);
        
        return value;
    }
}
