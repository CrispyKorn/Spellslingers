using UnityEngine;
using System;
using Unity.Netcode;

[Serializable]
public class CardValues : IEquatable<CardValues>, INetworkSerializable
{
    public int Power { get => _power; set => _power = value; }
    public int Special { get => _special; set => _special = value; }

    [SerializeField, Range(0, 5)] private int _power;
    [SerializeField, Range(0, 3)] private int _special;

    public CardValues() {}

    public CardValues(int power, int special)
    {
        _power = power;
        _special = special;
    }

    /// <summary>
    /// Sets all values to zero.
    /// </summary>
    public void Zero()
    {
        _power = 0;
        _special = 0;
    }

    public bool Equals(CardValues other)
    {
        return _power == other.Power && _special == other.Special;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _power);
        serializer.SerializeValue(ref _special);
    }

    public static CardValues operator +(CardValues left, CardValues right)
    {
        return new CardValues(left.Power + right.Power, left.Special + right.Special);
    }

    public static CardValues operator -(CardValues left, CardValues right)
    {
        return new CardValues(left.Power - right.Power, left.Special - right.Special);
    }
}
