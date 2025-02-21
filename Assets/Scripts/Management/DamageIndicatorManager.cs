using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using TMPro;

public class DamageIndicatorManager : NetworkBehaviour
{
    [Serializable]
    private class IndicatorSet
    {
        public Indicator P1Holder { get => _p1Holder; }
        public Indicator P2Holder { get => _p2Holder; }

        [SerializeField] Indicator _p1Holder;
        [SerializeField] Indicator _p2Holder;

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

        [SerializeField] private TextMeshProUGUI _attackCounterText;
        [SerializeField] private TextMeshProUGUI _defenceCounterText;
        [SerializeField] private TextMeshProUGUI _specialAttackCounterText;
        [SerializeField] private TextMeshProUGUI _specialDefenceCounterText;

         private int _attackCounter;
         private int _defenceCounter;
         private int _specialAttackCounter;
         private int _specialDefenceCounter;

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
    }

    [Rpc(SendTo.Everyone)]
    public void SetIndicatorsRpc(bool forPlayer1, ICard.CardType cardType, Card.CardElement cardElement, CardValues cardValues)
    {
        Indicator indicator = null;
        
        // Find relevant holder and colour
        switch (cardElement)
        {
            case Card.CardElement.Water: 
            {
                indicator = forPlayer1 ? _waterIndicators.P1Holder : _waterIndicators.P2Holder;
            }
            break;
            case Card.CardElement.Fire: 
            {
                indicator = forPlayer1 ? _fireIndicators.P1Holder : _fireIndicators.P2Holder;
            }
            break;
            case Card.CardElement.Electricity: 
            {
                indicator = forPlayer1 ? _electricityIndicators.P1Holder : _electricityIndicators.P2Holder;
            }
            break;
        }
        
        // Set power and special indicators
        if (cardType == ICard.CardType.Defence)
        {
            indicator.DefenceCounter += cardValues.Power;
            indicator.SpecialDefenceCounter += cardValues.Special;
        }
        else
        {
            indicator.AttackCounter += cardValues.Power;
            indicator.SpecialAttackCounter += cardValues.Special;
        }

        indicator.UpdateText();
    }

    [Rpc(SendTo.Everyone)]
    public void ClearIndicatorsRpc()
    {
        _waterIndicators.ResetCounters();
        _fireIndicators.ResetCounters();
        _electricityIndicators.ResetCounters();
    }
}
