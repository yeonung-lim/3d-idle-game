using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public enum StatType
    {
        Health,
        MaxHealth,
        AttackPower,
        Defense,
        MoveSpeed,
        GoldBonus,
        ExpBonus
        // Add more stat types as needed
    }

    [System.Serializable]
    public class Stat
    {
        public StatType Type { get; private set; }
        public float Value { get; set; }

        public Stat(StatType type, float value)
        {
            this.Type = type;
            this.Value = value;
        }
    }
}
