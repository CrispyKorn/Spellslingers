using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using TMPro;
using System.Threading.Tasks;

public class DamageIndicatorManager : NetworkBehaviour
{
    [Serializable]
    private class IndicatorSet
    {
        public Indicator P1Holder { get => _p1Holder; }
        public Indicator P2Holder { get => _p2Holder; }

        [SerializeField] Indicator _p1Holder;
        [SerializeField] Indicator _p2Holder;

        public void Initialize()
        {
            _p1Holder.Initialize();
            _p2Holder.Initialize();
        }

        public void ResetCounters()
        {
            _p1Holder.ResetCounters();
            _p2Holder.ResetCounters();
        }
    }

    [Serializable]
    private class Indicator
    {
        public int AttackCounter { get => _attackCounter; set => _attackCounter = value; }
        public int DefenceCounter { get => _defenceCounter; set => _defenceCounter = value; }
        public int SpecialAttackCounter { get => _specialAttackCounter; set => _specialAttackCounter = value; }
        public int SpecialDefenceCounter { get => _specialDefenceCounter; set => _specialDefenceCounter = value; }
        public Animator AttackCounterAnimator { get => _attackCounterAnimator; }
        public Animator DefenceCounterAnimator { get => _defenceCounterAnimator; }
        public Animator SpecialAttackCounterAnimator { get => _specialAttackCounterAnimator; }
        public Animator SpecialDefenceCounterAnimator { get => _specialDefenceCounterAnimator; }

        [SerializeField] private TextMeshProUGUI _attackCounterText;
        [SerializeField] private TextMeshProUGUI _defenceCounterText;
        [SerializeField] private TextMeshProUGUI _specialAttackCounterText;
        [SerializeField] private TextMeshProUGUI _specialDefenceCounterText;
        private Animator _attackCounterAnimator;
        private Animator _defenceCounterAnimator;
        private Animator _specialAttackCounterAnimator;
        private Animator _specialDefenceCounterAnimator;

        private int _attackCounter;
        private int _defenceCounter;
        private int _specialAttackCounter;
        private int _specialDefenceCounter;

        public void Initialize()
        {
            _attackCounterAnimator = _attackCounterText.GetComponentInParent<Animator>();
            _defenceCounterAnimator = _defenceCounterText.GetComponentInParent<Animator>();
            _specialAttackCounterAnimator = _specialAttackCounterText.GetComponentInParent<Animator>();
            _specialDefenceCounterAnimator = _specialDefenceCounterText.GetComponentInParent<Animator>();
        }

        public void UpdateText()
        {
            _attackCounterText.text = _attackCounter.ToString();
            _defenceCounterText.text = _defenceCounter.ToString();
            _specialAttackCounterText.text = _specialAttackCounter.ToString();
            _specialDefenceCounterText.text = _specialDefenceCounter.ToString();

            if (_attackCounter > 0) _attackCounterText.gameObject.SetActive(true);
            if (_defenceCounter > 0) _defenceCounterText.gameObject.SetActive(true);
            if (_specialAttackCounter > 0) _specialAttackCounterText.gameObject.SetActive(true);
            if (_specialDefenceCounter > 0) _specialDefenceCounterText.gameObject.SetActive(true);
        }

        public void ResetCounters()
        {
            _attackCounter = 0;
            _defenceCounter = 0;
            _specialAttackCounter = 0;
            _specialDefenceCounter = 0;

            _attackCounterText.gameObject.SetActive(false);
            _defenceCounterText.gameObject.SetActive(false);
            _specialAttackCounterText.gameObject.SetActive(false);
            _specialDefenceCounterText.gameObject.SetActive(false);

            UpdateText();
        }
    }
    
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
    public async Task AnimateIndicator(bool p1Indicator, Card.CardElement element, bool isAttack, bool isSpecial)
    {
        TriggerClientAnimationRpc(p1Indicator, element, isAttack, isSpecial);
        await PlayIndicator(p1Indicator, element, isAttack, isSpecial);
    }

    /// <summary>
    /// Plays the indicator animation, waiting for the animation to finish before returning. Run by player 1 (host) only.
    /// </summary>
    /// <param name="p1Indicator">Whether the indicator is for player 1.</param>
    /// <param name="element">The element of the indicator.</param>
    /// <param name="isAttack">Whether the indicator is an attack indicator.</param>
    /// <param name="isSpecial">Whether the indicator is a special indicator.</param>
    /// <returns></returns>
    private async Task PlayIndicator(bool p1Indicator, Card.CardElement element, bool isAttack, bool isSpecial)
    {
        Indicator indicator = GetIndicator(p1Indicator, element);
        Animator animator = isAttack ? 
                                (isSpecial ? indicator.SpecialAttackCounterAnimator : indicator.AttackCounterAnimator) : 
                                (isSpecial ? indicator.SpecialDefenceCounterAnimator : indicator.DefenceCounterAnimator);

        animator.SetTrigger("Activate");
        await Awaitable.NextFrameAsync();

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("IndicatorIdle"))
        {
            await Awaitable.NextFrameAsync();
        }
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
}
