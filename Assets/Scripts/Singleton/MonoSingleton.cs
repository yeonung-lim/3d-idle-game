using System.Collections;
using System.Collections.Generic;

using UnityCommunity.UnitySingleton;

using UnityEngine;

namespace UnityCommunity.UnitySingleton
{

    /// <summary>
    /// 기본 MonoBehaviour 싱글톤 구현, 이 싱글톤은 씬 변경 후 파괴됩니다. 
    /// 지속적이고 전역적인 싱글톤 인스턴스를 원한다면 <see cref="PersistentMonoSingleton{T}"/>를 사용하세요
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
    {

        #region Fields

        /// <summary>
        /// 인스턴스
        /// </summary>
        private static T instance;

        /// <summary>
        /// 싱글톤 인스턴스의 초기화 상태
        /// </summary>
        private SingletonInitializationStatus initializationStatus = SingletonInitializationStatus.None;

        #endregion

        #region Properties

        /// <summary>
        /// 인스턴스를 가져옵니다.
        /// </summary>
        /// <value>인스턴스</value>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                        instance.OnMonoSingletonCreated();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 싱글톤 인스턴스가 초기화되었는지 여부를 가져옵니다.
        /// </summary>
        public virtual bool IsInitialized => this.initializationStatus == SingletonInitializationStatus.Initialized;

        #endregion

        #region Unity Messages

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;

                // Initialize existing instance
                InitializeSingleton();
            }
            else
            {

                // Destory duplicates
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 싱글톤 인스턴스가 생성되면 호출됩니다.
        /// </summary>
        protected virtual void OnMonoSingletonCreated()
        {

        }

        protected virtual void OnInitializing()
        {

        }

        protected virtual void OnInitialized()
        {

        }

        #endregion

        #region Public Methods

        public virtual void InitializeSingleton()
        {
            if (this.initializationStatus != SingletonInitializationStatus.None)
            {
                return;
            }

            this.initializationStatus = SingletonInitializationStatus.Initializing;
            OnInitializing();
            this.initializationStatus = SingletonInitializationStatus.Initialized;
            OnInitialized();
        }

        public virtual void ClearSingleton() { }

        public static void CreateInstance()
        {
            DestroyInstance();
            instance = Instance;
        }

        public static void DestroyInstance()
        {
            if (instance == null)
            {
                return;
            }

            instance.ClearSingleton();
            instance = default(T);
        }

        #endregion

    }

}