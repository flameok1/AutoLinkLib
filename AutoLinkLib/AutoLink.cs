//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AutoLinkLib
{

    public class AutoLinkLib
    {
        /// <summary>
        /// 新增AutoLinkLib到Menu Tools 中，點擊會呼叫此Function
        /// </summary>
        [UnityEditor.MenuItem("Tools/AutoLinkLib/AutoLinkVar")]
        static void MenuAutoLinkVar()
        {
            if (UnityEditor.Selection.activeGameObject != null)
            {
                Component[] allComponents = UnityEditor.Selection.activeGameObject.GetComponents<Component>();

                foreach (Component selComponent in allComponents)
                {
                    if (selComponent.GetType() == typeof(RectTransform))
                        continue;

                    if (selComponent.GetType() == typeof(Transform))
                        continue;

                    if (selComponent.GetType() == typeof(UnityEngine.Canvas))
                        continue;

                    if (selComponent.GetType() == typeof(UnityEngine.CanvasGroup))
                        continue;

                    AutoLinkVar(selComponent, selComponent.GetType(), UnityEditor.Selection.activeGameObject.transform);

                    EditorUtility.SetDirty(selComponent);
                }

                EditorUtility.SetDirty(UnityEditor.Selection.activeGameObject);
            }
        }

        /// <summary>
        /// 自動連結主體
        /// </summary>
        public static void AutoLinkVar(object target, System.Type targetType, Transform targetTrans)
        {
            foreach (System.Reflection.FieldInfo targetFieldInfo in targetType.GetFields(System.Reflection.BindingFlags.Public |
                                                                       System.Reflection.BindingFlags.NonPublic |
                                                                       System.Reflection.BindingFlags.Instance))
            {
                //陣列和單一物件分開處理
                if (targetFieldInfo.FieldType.IsArray)
                {
                    FindAndLinkArray(target, targetTrans, targetFieldInfo);
                }
                else
                    FindAndLink(target, targetTrans, targetFieldInfo);
            }

#if UNITY_EDITOR
        if (target as UnityEngine.Object != null)
            UnityEditor.EditorUtility.SetDirty(target as UnityEngine.Object);
#endif
        }

        /// <summary>
        /// 處理非陣列物件
        /// </summary>
        public static void FindAndLink(object target, Transform targetTrans, System.Reflection.FieldInfo targetFieldInfo)
        {
            if (targetTrans == null)
                return;

            string sName = targetFieldInfo.Name;

            if (sName.StartsWith("_")) //僅支援一個底線
                sName = sName.Substring(1);

            Transform tFind = FindName(targetTrans, sName, false);
            if (tFind != null)
            {
                if (targetFieldInfo.FieldType == typeof(GameObject))
                {
                    targetFieldInfo.SetValue(target, tFind.gameObject);
                }
                else if(targetFieldInfo.FieldType == typeof(Vector3)) //有碰到再補吧
                {

                }
                else if (tFind.GetComponent(targetFieldInfo.FieldType) != null)
                {
                    targetFieldInfo.SetValue(target, tFind.GetComponent(targetFieldInfo.FieldType));
                }
            }
        }

        /// <summary>
        /// 處理陣列物件
        /// </summary>
        public static void FindAndLinkArray(object target, Transform targetTrans, System.Reflection.FieldInfo targetFieldInfo)
        {
            if (targetTrans == null)
                return;

            //陣列對應有固定格式 ex: btn00, btn01
            string fullName = targetFieldInfo.FieldType.FullName.Substring(0, targetFieldInfo.FieldType.FullName.Length - 2);
            System.Type elementType = System.Type.GetType(string.Format("{0},{1}", fullName, targetFieldInfo.FieldType.Assembly.GetName().Name));

            ArrayList constants = new ArrayList();
            List<Component> components = new List<Component>();
            Transform transFind = null;
            string sName = targetFieldInfo.Name;

            if (sName.StartsWith("_")) //僅支援一個底線
                sName = sName.Substring(1);

            for (int i = 0; ; ++i)
            {
                transFind = FindName(targetTrans, sName + i, false);
                if (transFind == null)
                    break;
                else
                {
                    if (elementType == typeof(GameObject))
                    {
                        constants.Add(transFind.gameObject);
                    }
                    else if (transFind.GetComponent(elementType) != null)
                    {
                        constants.Add(transFind.GetComponent(elementType));
                    }
                }
            }

            if (elementType != typeof(Vector3)) //有碰到再補吧
                targetFieldInfo.SetValue(target, constants.ToArray(elementType));
        }

        /// <summary>
        /// 找尋目標物件和子物件中同名物件, 可以只限定第一層不找子物件(bFirstLayer)
        /// </summary>
        public static Transform FindName(Transform targetTrans, string findName, bool bFirstLayer)
        {
            if (targetTrans == null)
                return null;

            Transform transFind = targetTrans.Find(findName);
            if (transFind != null)
            {
                return transFind;
            }

            if (bFirstLayer)
                return null;

            for (int i = 0; i < targetTrans.childCount; ++i)
            {
                transFind = targetTrans.GetChild(i);
                if (transFind.childCount > 0)
                {
                    transFind = FindName(transFind, findName, bFirstLayer);
                    if (transFind != null)
                        return transFind;
                }
            }
            return null;
        }

        /// <summary>
        /// 會回傳符合prename + {0~n}的物件 ，template T
        /// </summary>
        public static T[] GetArraysLink<T>(Transform targetTrans, string prename, bool bFirstLayer)
        {
            List<T> listArrays = new List<T>();
            for (int i = 0; ; ++i)
            {
                string[] findnames = string.Format(prename, i).Split('/');
                Transform transFind = null;
                if (findnames.Length > 1)
                {
                    transFind = targetTrans;
                    for (int j = 0; j < findnames.Length; ++j)
                    {
                        transFind = transFind.Find(findnames[j]);
                        if (transFind == null)
                            break;
                    }
                }
                else
                    transFind = FindName(targetTrans, findnames[0], bFirstLayer);
                if (transFind != null)
                {
                    T tBtns = transFind.GetComponent<T>();
                    if (tBtns != null)
                    {
                        listArrays.Add(tBtns);
                    }
                }
                else
                    break;
            }

            if (listArrays.Count > 0)
                return listArrays.ToArray();

            return null;
        }

        /// <summary>
        /// 會回傳符合prename + {0~n}的物件 ，GameObject only
        /// </summary>
        public static GameObject[] GetArraysLinkObj(Transform targetTrans, string prename, bool bFirstLayer)
        {
            List<GameObject> listArrays = new List<GameObject>();
            for (int i = 0; ; ++i)
            {
                string[] findnames = string.Format(prename, i).Split('/');
                Transform transFind = null;

                if (findnames.Length > 1)
                {
                    transFind = targetTrans.Find(string.Format(prename, i));
                }
                else
                    transFind = FindName(targetTrans, findnames[0], bFirstLayer);

                if (transFind != null)
                {
                    listArrays.Add(transFind.gameObject);
                }
                else
                    break;
            }

            return listArrays.ToArray();
        }
    }
}
