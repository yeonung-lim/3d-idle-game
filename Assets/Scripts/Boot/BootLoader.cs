using Sequence;
using UnityEngine;

/// <summary>
///     게임 첫 실행 시 초기화 작업 진행
/// </summary>
public class BootLoader : MonoBehaviour
{
    /// <summary>
    ///     게임 시퀀스 관리자
    /// </summary>
    [SerializeField] private SequenceManager sequenceManager;

    private void Awake()
    {
        Instantiate(sequenceManager).Initialize();
    }
}