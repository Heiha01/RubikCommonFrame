using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilsModule{
    public abstract class ManagerBase<T> : MonoBehaviour where T : ManagerBase<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = (T)GameObject.FindAnyObjectByType(typeof(T));
                    if (!_instance)
                    {
                        var gameOb = new GameObject(typeof(T).Name);
                        _instance = gameOb.AddComponent<T>();
                        DontDestroyOnLoad(gameOb);
                    }
                }
                return _instance;
            }
        }

        public abstract void Init();
    }
}
