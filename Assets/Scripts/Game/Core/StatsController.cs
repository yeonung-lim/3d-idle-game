using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class StatsController : MonoBehaviour
    {
        public Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

        /// <summary>
        /// Initializes the stats dictionary with a given list of stats.
        /// Clears any existing stats.
        /// </summary>
        /// <param name="initialStats">A list of Stat objects to initialize with.</param>
        public void InitializeStats(List<Stat> initialStats)
        {
            stats.Clear();
            if (initialStats != null)
            {
                foreach (var stat in initialStats)
                {
                    if (stat != null)
                    {
                        if (!stats.ContainsKey(stat.Type))
                        {
                            stats.Add(stat.Type, stat);
                        }
                        else
                        {
                            Debug.LogWarning($"StatsController: Stat type {stat.Type} already exists. Overwriting with new initial value.");
                            stats[stat.Type] = stat;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of a specified stat.
        /// </summary>
        /// <param name="statType">The type of stat to retrieve.</param>
        /// <returns>The value of the stat, or 0f if the stat doesn't exist.</returns>
        public float GetStatValue(StatType statType)
        {
            if (stats.TryGetValue(statType, out Stat stat))
            {
                return stat.Value;
            }
            else
            {
                Debug.LogWarning($"StatsController: Stat type {statType} not found. Returning default value 0f.");
                return 0f;
            }
        }

        /// <summary>
        /// Sets the value of an existing stat. If the stat doesn't exist, it adds it.
        /// </summary>
        /// <param name="statType">The type of stat to set.</param>
        /// <param name="value">The new value for the stat.</param>
        public void SetStatValue(StatType statType, float value)
        {
            if (stats.TryGetValue(statType, out Stat stat))
            {
                stat.Value = value;
            }
            else
            {
                Debug.LogWarning($"StatsController: Stat type {statType} not found. Adding it with the specified value.");
                AddStat(statType, value);
            }
        }

        /// <summary>
        /// Modifies the value of a stat by a given amount.
        /// </summary>
        /// <param name="statType">The type of stat to modify.</param>
        /// <param name="amount">The amount to add (can be negative to subtract).</param>
        public void ModifyStatValue(StatType statType, float amount)
        {
            if (stats.TryGetValue(statType, out Stat stat))
            {
                stat.Value += amount;
            }
            else
            {
                Debug.LogWarning($"StatsController: Stat type {statType} not found. Cannot modify. Consider adding it first.");
            }
        }

        /// <summary>
        /// Adds a new stat or updates an existing one.
        /// </summary>
        /// <param name="newStat">The Stat object to add or update.</param>
        public void AddStat(Stat newStat)
        {
            if (newStat == null)
            {
                Debug.LogError("StatsController: Cannot add a null stat.");
                return;
            }

            if (stats.ContainsKey(newStat.Type))
            {
                stats[newStat.Type] = newStat;
                // Alternatively, could update stats[newStat.Type].Value = newStat.Value;
                // Depending on whether a full replacement or just value update is desired.
                // For now, replacing the whole Stat object.
                Debug.Log($"StatsController: Stat type {newStat.Type} already exists. Updated with new stat object.");
            }
            else
            {
                stats.Add(newStat.Type, newStat);
            }
        }

        /// <summary>
        /// Convenience method to add a new stat with a specific type and value.
        /// If the stat type already exists, its value will be updated.
        /// </summary>
        /// <param name="statType">The type of stat to add.</param>
        /// <param name="value">The initial value for the stat.</param>
        public void AddStat(StatType statType, float value)
        {
            if (stats.ContainsKey(statType))
            {
                stats[statType].Value = value;
                Debug.Log($"StatsController: Stat type {statType} already exists. Updated value to {value}.");
            }
            else
            {
                stats.Add(statType, new Stat(statType, value));
            }
        }
    }
}
