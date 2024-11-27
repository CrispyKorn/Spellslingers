
using UnityEngine;
using System;

[Serializable]
public class CardValues
{
    public int Power { get => _power; set => _power = value; }
    public int Special { get => _special; set => _special = value; }

    [SerializeField, Range(0, 5)] private int _power;
    [SerializeField, Range(0, 3)] private int _special;
}
