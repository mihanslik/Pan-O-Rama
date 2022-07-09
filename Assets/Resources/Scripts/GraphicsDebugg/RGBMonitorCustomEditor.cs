using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RGBMonitor))]
public class RGBMonitorCustomEditor : Editor
{
    private int _channelOption = 0;
    private int _lastChannelOption = 0;
    private string[] _channelNames = { "RGB", "R", "G", "B" };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RGBMonitor targetScript = (RGBMonitor)target;
        
        _channelOption = GUILayout.Toolbar(_channelOption, _channelNames);
        if (_channelOption != _lastChannelOption)
        {
            targetScript.SetChannelSplit(_channelOption);
            _lastChannelOption = _channelOption;
        }

        if (targetScript.OutputTexture != null)
        {
            GUILayout.Space(targetScript.OutputTexture.height);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawPreviewTexture(new Rect(lastRect.x +1, lastRect.y, targetScript.OutputTexture.width, targetScript.OutputTexture.height), targetScript.OutputTexture);
        }
        if (targetScript.ReferenceInputTexture != null && targetScript.OutputTexture_Ref != null)
        {
            GUILayout.Space(targetScript.OutputTexture_Ref.height);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawPreviewTexture(new Rect(lastRect.x + 1, lastRect.y, targetScript.OutputTexture_Ref.width, targetScript.OutputTexture_Ref.height), targetScript.OutputTexture_Ref);
        }



    }
}
