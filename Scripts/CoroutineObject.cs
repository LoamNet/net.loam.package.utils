using UnityEngine;

namespace Loam
{
    /// <summary>
    /// Provides a global object to host coroutines on, which can be useful for starting
    /// coroutines from static scopes or classes that don't derive from MonoBehaviour.
    /// This class does no coroutine management past default unity behavior. 
    /// 
    /// To use:
    ///   CoroutineObject.Instance.StartCoroutine(...);
    ///   
    /// </summary>
    public class CoroutineObject : MonoBehaviour
    {
        private static CoroutineObject instance;
        public static CoroutineObject Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject thisObj = new GameObject("Global Coroutine Object");
                    instance = thisObj.AddComponent<CoroutineObject>();
                    DontDestroyOnLoad(thisObj);
                }

                return instance;
            }
        }
    }
}