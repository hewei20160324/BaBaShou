using UnityEngine;
using System.Collections;

namespace CustomUtil
{
    public class UnityCustomUtil
    {
        #region log
        public static void CustomLog(string str)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(str);
#else
            if(Debug.isDebugBuild)
            {
                UnityEngine.Debug.Log(str);
            }
#endif
        }

        public static void CustomLogWarning(string str)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(str);
#else
            if(Debug.isDebugBuild)
            {
                UnityEngine.Debug.LogWarning(str);
            }
#endif
        }

        public static void CustomLogError(string str)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(str);
#else
            if(Debug.isDebugBuild)
            {
                UnityEngine.Debug.LogError(str);
            }
#endif
        }
        #endregion

        #region component

        /// <summary>
        /// get component, of not exist will add component;
        /// </summary>

        public static T GetComponentAddIfNotExist<T>(GameObject obj) where T : Component
        {
            if (obj == null) return null;

            T t = obj.GetComponent<T>();
            if (t == null) t = obj.AddComponent<T>();

            return t;
        }

        /// <summary>
        /// get child gameObject;
        /// </summary>

        public static GameObject GetChild(GameObject go, string name, bool includeSelf = false)
        {
            if (go == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (includeSelf && go.name == name)
            {
                return go;
            }

            Transform tranChild = go.transform.FindChild(name);
            if (tranChild != null)
            {
                return tranChild.gameObject;
            }
            return null;
        }

        /// <summary>
        /// get child component;
        /// </summary>

        public static T GetChildComponent<T>(GameObject go, string name) where T : Component
        {
            GameObject childObj = GetChild(go, name);
            if (childObj == null) return null;

            T t = childObj.GetComponent<T>();
            return t;
        }

        /// <summary>
        /// 删除所有子结点;
        /// </summary>

        public static void DeleteAllChildren(GameObject obj)
        {
            if (obj == null) return;

            Transform trans = obj.transform;
            for(int nIdx = trans.childCount - 1; nIdx >= 0; nIdx--)
            {
                Transform tempTrans = trans.GetChild(nIdx);
                if(tempTrans != null)
                {
                    GameObject.Destroy(tempTrans.gameObject);
                }
            }
        }
        #endregion
    }
}
