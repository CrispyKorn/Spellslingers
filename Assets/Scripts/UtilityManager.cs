using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UtilityManager : MonoBehaviour
{
    public struct UtilityInfo
    {
        public CardManager cardManager;
        public Player player1, player2;
        public bool activatedByPlayer1;

        public UtilityInfo(CardManager _cardManager, Player _player1, Player _player2, bool _activatedByPlayer1 = false)
        {
            cardManager = _cardManager;
            player1 = _player1;
            player2 = _player2;
            activatedByPlayer1 = _activatedByPlayer1;
        }
    }

    UtilityInfo utilityInfo;

    public void Initialize(UtilityInfo _utilityInfo)
    {
        utilityInfo = _utilityInfo;
    }

    public void ApplyUtilityEffect(UtilityCard utilityCard, bool _activatedByPlayer1)
    {
        utilityInfo.activatedByPlayer1 = _activatedByPlayer1;
        utilityCard.ApplyEffect(utilityInfo);
    }
}
