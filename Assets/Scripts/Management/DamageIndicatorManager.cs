using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class DamageIndicatorManager : NetworkBehaviour
{
    [Serializable]
    private class HolderSet
    {
        public Holder P1Holder { get => _p1Holder; }
        public Holder P2Holder { get => _p2Holder; }

        [SerializeField] Holder _p1Holder;
        [SerializeField] Holder _p2Holder;

        public void ClearIndicators()
        {
            _p1Holder.ClearIndicators();
            _p2Holder.ClearIndicators();
        }
    }

    [Serializable]
    private class Holder
    {
        public Transform HolderTransform { get => _holder; }
        public List<GameObject> PowerIndicators { get => _powerIndicators; }
        public List<GameObject> SpecialIndicators { get => _specialIndicators; }

        [SerializeField] private Transform _holder;

        private List<GameObject> _powerIndicators = new();
        private List<GameObject> _specialIndicators = new();

        public void ClearIndicators()
        {
            foreach (GameObject indicator in _powerIndicators) Destroy(indicator);
            foreach (GameObject indicator in _specialIndicators) Destroy(indicator);

            _powerIndicators.Clear();
            _specialIndicators.Clear();
        }
    }

    [SerializeField] private GameObject _powerPrefab;
    [SerializeField] private GameObject _specialPrefab;
    [SerializeField] private Color _waterColour = Color.blue;
    [SerializeField] private Color _fireColour = Color.red;
    [SerializeField] private Color _electricityColour = Color.yellow;
    [SerializeField] private HolderSet _waterHolders;
    [SerializeField] private HolderSet _fireHolders;
    [SerializeField] private HolderSet _electricityHolders;
    [SerializeField] private float _indicatorHeight = 1f;

    private void Awake()
    {
        Locator.Instance.RegisterInstance(this);
    }

    [Rpc(SendTo.Everyone)]
    public void AddIndicatorsRpc(bool forPlayer1, Card.CardElement cardElement, CardValues cardValues)
    {
        Holder holder = null;
        Color colour = Color.white;
        
        // Find relevant holder and colour
        switch (cardElement)
        {
            case Card.CardElement.Water: 
            {
                holder = forPlayer1 ? _waterHolders.P1Holder : _waterHolders.P2Holder; 
                colour = _waterColour;
            }
            break;
            case Card.CardElement.Fire: 
            {
                holder = forPlayer1 ? _fireHolders.P1Holder : _fireHolders.P2Holder; 
                colour = _fireColour;
            }
            break;
            case Card.CardElement.Electricity: 
            {
                holder = forPlayer1 ? _electricityHolders.P1Holder : _electricityHolders.P2Holder; 
                colour = _electricityColour;
            }
            break;
        }
        
        // Add power and special indicators
        for (int i = 0; i < cardValues.Power; i++)
        {
            GameObject powerIndicator = Instantiate(_powerPrefab, holder.HolderTransform);
            powerIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = colour;
            holder.PowerIndicators.Add(powerIndicator);
        }

        for (int i = 0; i < cardValues.Special; i++)
        {
            GameObject specialIndicator = Instantiate(_specialPrefab, holder.HolderTransform);
            specialIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = colour;
            holder.SpecialIndicators.Add(specialIndicator);
        }

        // Sort/Layout Indicators
        for (int i = 0; i < holder.SpecialIndicators.Count; i++)
        {
            Transform indicatorTransform = holder.SpecialIndicators[i].transform;
            indicatorTransform.localPosition = new Vector3(indicatorTransform.localPosition.x, i * -_indicatorHeight, 0f);
        }

        for (int i = 0; i < holder.PowerIndicators.Count; i++)
        {
            Transform indicatorTransform = holder.PowerIndicators[i].transform;
            int specialCount = holder.SpecialIndicators.Count;

            indicatorTransform.localPosition = new Vector3(indicatorTransform.localPosition.x, (specialCount + i) * -_indicatorHeight, 0f);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ClearIndicatorsRpc()
    {
        _waterHolders.ClearIndicators();
        _fireHolders.ClearIndicators();
        _electricityHolders.ClearIndicators();
    }
}
