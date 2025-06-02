using System;

namespace Utils
{
    /// <summary>
    /// 숫자 관련 유틸리티 함수들을 제공하는 정적 클래스
    /// </summary>
    public static class NumberUtils
    {
        /// <summary>
        /// 큰 숫자를 읽기 쉬운 형태로 포맷팅합니다 (예: 1500 -> "1.5K")
        /// </summary>
        /// <param name="number">포맷팅할 숫자</param>
        /// <param name="decimalPlaces">소수점 자릿수 (기본값: 1)</param>
        /// <returns>포맷팅된 문자열</returns>
        public static string FormatLargeNumber(long number, int decimalPlaces = 1)
        {
            if (number == 0) return "0";

            string[] suffixes = { "", "K", "M", "B", "T", "Q" };
            int suffixIndex = 0;
            double value = Math.Abs(number);

            while (value >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1000;
                suffixIndex++;
            }

            string format = $"F{decimalPlaces}";
            string formattedValue = value.ToString(format);

            // 불필요한 소수점 제거
            if (decimalPlaces > 0)
            {
                formattedValue = formattedValue.TrimEnd('0').TrimEnd('.');
            }

            string result = formattedValue + suffixes[suffixIndex];
            return number < 0 ? "-" + result : result;
        }

        /// <summary>
        /// 큰 숫자를 읽기 쉬운 형태로 포맷팅합니다 (float 버전)
        /// </summary>
        /// <param name="number">포맷팅할 숫자</param>
        /// <param name="decimalPlaces">소수점 자릿수 (기본값: 1)</param>
        /// <returns>포맷팅된 문자열</returns>
        public static string FormatLargeNumber(float number, int decimalPlaces = 1)
        {
            return FormatLargeNumber((long)number, decimalPlaces);
        }

        /// <summary>
        /// 큰 숫자를 읽기 쉬운 형태로 포맷팅합니다 (double 버전)
        /// </summary>
        /// <param name="number">포맷팅할 숫자</param>
        /// <param name="decimalPlaces">소수점 자릿수 (기본값: 1)</param>
        /// <returns>포맷팅된 문자열</returns>
        public static string FormatLargeNumber(double number, int decimalPlaces = 1)
        {
            return FormatLargeNumber((long)number, decimalPlaces);
        }

        /// <summary>
        /// 숫자를 천 단위로 구분하여 포맷팅합니다 (예: 1234567 -> "1,234,567")
        /// </summary>
        /// <param name="number">포맷팅할 숫자</param>
        /// <returns>천 단위로 구분된 문자열</returns>
        public static string FormatWithCommas(long number)
        {
            return number.ToString("N0");
        }

        /// <summary>
        /// 퍼센트 값을 포맷팅합니다 (예: 0.75f -> "75%")
        /// </summary>
        /// <param name="percentage">0~1 범위의 퍼센트 값</param>
        /// <param name="decimalPlaces">소수점 자릿수 (기본값: 0)</param>
        /// <returns>포맷팅된 퍼센트 문자열</returns>
        public static string FormatPercentage(float percentage, int decimalPlaces = 0)
        {
            float percent = percentage * 100f;
            string format = $"F{decimalPlaces}";
            return percent.ToString(format) + "%";
        }

        /// <summary>
        /// 시간을 분:초 형태로 포맷팅합니다 (예: 125 -> "2:05")
        /// </summary>
        /// <param name="totalSeconds">총 초</param>
        /// <returns>분:초 형태의 문자열</returns>
        public static string FormatTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:D2}";
        }

        /// <summary>
        /// 값을 지정된 범위 내로 제한합니다.
        /// </summary>
        /// <param name="value">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>제한된 값</returns>
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 값을 지정된 범위 내로 제한합니다 (int 버전)
        /// </summary>
        /// <param name="value">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>제한된 값</returns>
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
} 