using System;
using UnityEngine;

[Serializable]
public class PlayerStat
{
    public delegate void PlayerStatChangedEvent(PlayerStat playerStat);

    [SerializeField] private float currentValue;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;

    public event PlayerStatChangedEvent OnStatChanged;


    public float MaxValue
    {
        get { return maxValue; }
    }

    public float MinValue
    {
        get { return minValue; }
    }

    public float Value
    {
        get { return currentValue; }
        set 
        {
            float newValue = Mathf.Clamp(value, minValue, maxValue);
            bool isDifferent = newValue != currentValue;
            currentValue = newValue;
            if (isDifferent)
                OnStatChanged?.Invoke(this);
        }
    }

    public static implicit operator float(PlayerStat playerStat) => playerStat.Value;
}
