using System.Collections;
using UnityEngine;

/// <summary>
///     유틸리티 코루틴 메서드를 캡슐화합니다.
///     이 클래스를 사용하면 모노비헤이비어가 아닌 클래스에서 코루틴을 실행할 수 있습니다.
///     숨겨진 게임 오브젝트를 인스턴스화하고 코루틴을 시작/중지하는 데 사용되는 빈 모노비헤이비어 구성 요소를
///     여기에 추가합니다.
/// </summary>
public static class Coroutines
{
    private static MonoBehaviour s_Instance;

    private static MonoBehaviour Instance
    {
        get
        {
            if (s_Instance == null)
            {
                var instance = new GameObject(nameof(Coroutines), typeof(CoroutineHelper));
                s_Instance = instance.GetComponent<CoroutineHelper>();
                instance.hideFlags = HideFlags.HideAndDontSave;
                Object.DontDestroyOnLoad(instance);
            }

            return s_Instance;
        }
    }

    /// <summary>
    ///     코루틴 시작
    /// </summary>
    /// <param name="routine">코루틴 시작하기</param>
    /// <returns>시작된 코루틴</returns>
    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }

    /// <summary>
    ///     코루틴을 중지합니다.
    /// </summary>
    /// <param name="coroutine">멈춰야 할 코루틴</param>
    public static void StopCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            Instance.StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    public class CoroutineHelper : MonoBehaviour
    {
    }
}