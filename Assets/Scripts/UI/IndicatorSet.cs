using UnityEngine;
using System;

[Serializable]
public class IndicatorSet
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
