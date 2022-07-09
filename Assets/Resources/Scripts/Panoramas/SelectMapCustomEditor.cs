using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(SelectMap)), CanEditMultipleObjects]
public class SelectMapCustomEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SelectMap targetScript = (SelectMap)target;

        GUILayout.Space(20);
        GUI.backgroundColor = Color.grey;
        foreach (MapVariant variant in (MapVariant[])Enum.GetValues(typeof(MapVariant)))
        {
            if (GUILayout.Button(variant.ToString()))
                targetScript.ActivateMap(variant);
        }
    }

}
