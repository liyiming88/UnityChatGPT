using UnityEngine;
namespace MotionverseSDK.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {

        private static T sInstance = null;

        public static T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject gameObject = new(typeof(T).FullName);
                    sInstance = gameObject.AddComponent<T>();
                }

                return sInstance;
            }
        }

        public static void Clear()
        {
            sInstance = null;
        }

        protected virtual void Awake()
        {
            if (sInstance != null) Debug.LogError(name + "error: already initialized", this);

            sInstance = (T)this;
        }
    }
}