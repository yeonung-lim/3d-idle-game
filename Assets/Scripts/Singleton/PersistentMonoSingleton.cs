using System.Collections;
using System.Collections.Generic;

using UnityCommunity.UnitySingleton;

using UnityEngine;

namespace UnityCommunity.UnitySingleton
{

    /// <summary>
    /// 이 싱글톤은 <see cref="UnityEngine.Object.DontDestroyOnLoad(Object)"/>를 호출하여 씬 간에 지속됩니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
    {

        #region Protected Methods

        protected override void OnInitializing()
        {
            base.OnInitializing();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        #endregion

    }

}