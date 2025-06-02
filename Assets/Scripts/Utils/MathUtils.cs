using UnityEngine;
using System;

namespace Utils
{
    /// <summary>
    /// 수학 관련 유틸리티 함수들을 제공하는 정적 클래스
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// 주어진 확률에 따라 성공/실패를 판정합니다.
        /// </summary>
        /// <param name="probability">성공 확률 (0~100)</param>
        /// <returns>성공 여부</returns>
        public static bool RollProbability(float probability)
        {
            if (probability <= 0f) return false;
            if (probability >= 100f) return true;
            
            float roll = UnityEngine.Random.Range(0f, 100f);
            return roll < probability;
        }

        /// <summary>
        /// 주어진 확률에 따라 성공/실패를 판정합니다 (0~1 범위)
        /// </summary>
        /// <param name="probability">성공 확률 (0~1)</param>
        /// <returns>성공 여부</returns>
        public static bool RollProbabilityNormalized(float probability)
        {
            if (probability <= 0f) return false;
            if (probability >= 1f) return true;
            
            float roll = UnityEngine.Random.Range(0f, 1f);
            return roll < probability;
        }

        /// <summary>
        /// 가중치 배열에서 랜덤하게 인덱스를 선택합니다.
        /// </summary>
        /// <param name="weights">가중치 배열</param>
        /// <returns>선택된 인덱스</returns>
        public static int WeightedRandomChoice(float[] weights)
        {
            if (weights == null || weights.Length == 0)
                return -1;

            float totalWeight = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                totalWeight += weights[i];
            }

            if (totalWeight <= 0f)
                return UnityEngine.Random.Range(0, weights.Length);

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                    return i;
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// 값을 다른 범위로 매핑합니다.
        /// </summary>
        /// <param name="value">매핑할 값</param>
        /// <param name="fromMin">원래 범위의 최솟값</param>
        /// <param name="fromMax">원래 범위의 최댓값</param>
        /// <param name="toMin">새 범위의 최솟값</param>
        /// <param name="toMax">새 범위의 최댓값</param>
        /// <returns>매핑된 값</returns>
        public static float MapRange(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        /// <summary>
        /// 각도를 0~360도 범위로 정규화합니다.
        /// </summary>
        /// <param name="angle">정규화할 각도</param>
        /// <returns>정규화된 각도</returns>
        public static float NormalizeAngle(float angle)
        {
            while (angle < 0f) angle += 360f;
            while (angle >= 360f) angle -= 360f;
            return angle;
        }

        /// <summary>
        /// 두 각도 사이의 최단 거리를 계산합니다.
        /// </summary>
        /// <param name="from">시작 각도</param>
        /// <param name="to">목표 각도</param>
        /// <returns>최단 각도 거리</returns>
        public static float AngleDifference(float from, float to)
        {
            float diff = NormalizeAngle(to) - NormalizeAngle(from);
            if (diff > 180f) diff -= 360f;
            else if (diff < -180f) diff += 360f;
            return diff;
        }
    }
} 