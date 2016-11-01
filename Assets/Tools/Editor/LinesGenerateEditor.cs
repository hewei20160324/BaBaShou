using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LinesGenerate))]
public class LinesGenerateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LinesGenerate linesGenerate = target as LinesGenerate;
        if (linesGenerate == null) return;

        if(GUILayout.Button("刷新"))
        {
            linesGenerate.genereateBoard = false;
        }
    }
}
