using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class StatsController : MonoBehaviour
    {
        public Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

        /// <summary>
        /// 주어진 스탯 목록으로 스탯 딕셔너리를 초기화합니다.
        /// 기존에 있던 모든 스탯을 제거합니다.
        /// </summary>
        /// <param name="initialStats">초기화할 Stat 객체 목록</param>
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
                            Debug.LogWarning($"StatsController: {stat.Type} 타입의 스탯이 이미 존재합니다. 새로운 초기값으로 덮어씁니다.");
                            stats[stat.Type] = stat;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 지정된 스탯의 값을 가져옵니다.
        /// </summary>
        /// <param name="statType">가져올 스탯의 타입</param>
        /// <returns>스탯의 값, 스탯이 존재하지 않을 경우 0f를 반환합니다.</returns>
        public float GetStatValue(StatType statType)
        {
            if (stats.TryGetValue(statType, out Stat stat))
            {
                return stat.Value;
            }
            else
            {
                Debug.LogWarning($"StatsController: {statType} 타입의 스탯을 찾을 수 없습니다. 기본값 0f를 반환합니다.");
                return 0f;
            }
        }

        /// <summary>
        /// 기존 스탯의 값을 설정합니다. 스탯이 존재하지 않을 경우 새로 추가합니다.
        /// </summary>
        /// <param name="statType">설정할 스탯의 타입</param>
        /// <param name="value">스탯의 새로운 값</param>
        public void SetStatValue(StatType statType, float value)
        {
            if (stats.TryGetValue(statType, out Stat stat))
            {
                stat.Value = value;
            }
            else
            {
                Debug.LogWarning($"StatsController: {statType} 타입의 스탯을 찾을 수 없습니다. 지정된 값으로 새로 추가합니다.");
                AddStat(statType, value);
            }
        }

        /// <summary>
        /// 스탯의 값을 주어진 양만큼 수정합니다.
        /// </summary>
        /// <param name="statType">수정할 스탯의 타입</param>
        /// <param name="amount">더할 양 (뺄셈의 경우 음수 사용)</param>
        public void ModifyStatValue(StatType statType, float amount)
        {
            if (stats.TryGetValue(statType, out Stat stat))
            {
                stat.Value += amount;
            }
            else
            {
                Debug.LogWarning($"StatsController: {statType} 타입의 스탯을 찾을 수 없습니다. 수정할 수 없습니다. 먼저 스탯을 추가하는 것을 고려하세요.");
            }
        }

        /// <summary>
        /// 새로운 스탯을 추가하거나 기존 스탯을 업데이트합니다.
        /// </summary>
        /// <param name="newStat">추가하거나 업데이트할 Stat 객체</param>
        public void AddStat(Stat newStat)
        {
            if (newStat == null)
            {
                Debug.LogError("StatsController: null 스탯은 추가할 수 없습니다.");
                return;
            }

            if (stats.ContainsKey(newStat.Type))
            {
                stats[newStat.Type] = newStat;
                // 선택적으로 stats[newStat.Type].Value = newStat.Value; 사용 가능
                // 전체 교체가 필요한지 값만 업데이트할지에 따라 다름
                // 현재는 전체 Stat 객체를 교체하는 방식 사용
                Debug.Log($"StatsController: {newStat.Type} 타입의 스탯이 이미 존재합니다. 새로운 스탯 객체로 업데이트했습니다.");
            }
            else
            {
                stats.Add(newStat.Type, newStat);
            }
        }

        /// <summary>
        /// 특정 타입과 값으로 새로운 스탯을 추가하는 편의 메서드입니다.
        /// 스탯 타입이 이미 존재하는 경우 값이 업데이트됩니다.
        /// </summary>
        /// <param name="statType">추가할 스탯의 타입</param>
        /// <param name="value">스탯의 초기값</param>
        public void AddStat(StatType statType, float value)
        {
            if (stats.ContainsKey(statType))
            {
                stats[statType].Value = value;
                Debug.Log($"StatsController: {statType} 타입의 스탯이 이미 존재합니다. 값을 {value}로 업데이트했습니다.");
            }
            else
            {
                stats.Add(statType, new Stat(statType, value));
            }
        }
    }
}
