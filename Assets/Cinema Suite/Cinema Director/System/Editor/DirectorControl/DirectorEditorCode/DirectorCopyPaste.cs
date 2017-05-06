using System;
using UnityEngine;

public class DirectorCopyPaste
{
    private static Behaviour clipboard;
    private static GameObject deepCopy;

    public static void Copy(Behaviour obj)
    {
        clipboard = obj;
        GameObject obj2 = clipboard.gameObject;
        deepCopy = GameObject.Instantiate(obj2) as GameObject;
        deepCopy.name = (DirectorControlHelper.GetNameForDuplicate(obj, obj2.name));
        deepCopy.hideFlags = (HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable);
    }

    public static GameObject Paste(Transform parent)
    {
        GameObject obj2 = null;
        if (clipboard != null)
        {
            obj2 = GameObject.Instantiate(deepCopy) as GameObject;
            obj2.name = (deepCopy.name);
            obj2.transform.parent = (parent);
        }
        return obj2;
    }

    public static Behaviour Peek()
    { return clipboard; }
}

