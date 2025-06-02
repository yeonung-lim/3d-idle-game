using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Transform 클래스의 확장 메소드를 제공하는 정적 클래스
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Transform의 X 위치만 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="x">설정할 X 좌표</param>
        public static void SetPositionX(this Transform transform, float x)
        {
            Vector3 position = transform.position;
            position.x = x;
            transform.position = position;
        }

        /// <summary>
        /// Transform의 Y 위치만 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="y">설정할 Y 좌표</param>
        public static void SetPositionY(this Transform transform, float y)
        {
            Vector3 position = transform.position;
            position.y = y;
            transform.position = position;
        }

        /// <summary>
        /// Transform의 Z 위치만 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="z">설정할 Z 좌표</param>
        public static void SetPositionZ(this Transform transform, float z)
        {
            Vector3 position = transform.position;
            position.z = z;
            transform.position = position;
        }

        /// <summary>
        /// Transform의 로컬 X 위치만 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="x">설정할 로컬 X 좌표</param>
        public static void SetLocalPositionX(this Transform transform, float x)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.x = x;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// Transform의 로컬 Y 위치만 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="y">설정할 로컬 Y 좌표</param>
        public static void SetLocalPositionY(this Transform transform, float y)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = y;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// Transform의 로컬 Z 위치만 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="z">설정할 로컬 Z 좌표</param>
        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.z = z;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// Transform의 스케일 값을 균등하게 설정합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        /// <param name="scale">설정할 스케일 값</param>
        public static void SetScale(this Transform transform, float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Transform을 원점으로 리셋합니다.
        /// </summary>
        /// <param name="transform">대상 Transform</param>
        public static void ResetTransform(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 두 Transform 사이의 거리를 반환합니다.
        /// </summary>
        /// <param name="transform">첫 번째 Transform</param>
        /// <param name="other">두 번째 Transform</param>
        /// <returns>두 Transform 사이의 거리</returns>
        public static float DistanceTo(this Transform transform, Transform other)
        {
            return Vector3.Distance(transform.position, other.position);
        }
    }
} 