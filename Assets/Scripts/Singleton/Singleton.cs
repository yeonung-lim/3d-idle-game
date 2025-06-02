using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UnityCommunity.UnitySingleton
{

    public enum SingletonInitializationStatus
    {
        None,
        Initializing,
        Initialized
    }

    /// <summary>
    /// 클래스를 위한 싱글톤 구현
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
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
                    //하나의 스레드만 실행할 수 있도록 보장
                    lock (typeof(T))
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            instance.InitializeSingleton();
                        }
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

        #region Protected Methods

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