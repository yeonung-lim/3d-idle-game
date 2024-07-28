using UnityEngine;
using UnityEngine.UI;
using USingleton;

namespace Sound
{
    /// <summary>
    ///     버튼 클릭 시 사운드를 재생하는 클래스입니다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().AddListener(this, () =>
                Singleton.Instance<AudioManager>().PlaySfx(UIAudioType.ButtonClick));
        }
    }
}