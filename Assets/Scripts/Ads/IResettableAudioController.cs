namespace Ads
{
    /// <summary>
    ///     리셋 가능 오디오 컨트롤러
    /// </summary>
    public interface IResettableAudioController
    {
        /// <summary>
        ///     리셋 하기 전 준비 (광고 송출 전)
        /// </summary>
        void ResetPrepare();

        /// <summary>
        ///     리셋 (광고 송출 후)
        /// </summary>
        void ResetAudio();
    }
}