using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GraphicsDebugger))]
public class GraphicsDebuggerCustomEditor : Editor
{
    private int _channelOption = 0;
    private int _lastChannelOption = 0;
    private string[] _channelNames = { "RGB", "R", "G", "B" };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GraphicsDebugger targetScript = (GraphicsDebugger)target;

        GUILayout.Space(20);

        GUILayout.Label("Channel split:");
        _channelOption = GUILayout.Toolbar(_channelOption, _channelNames);
        if(_channelOption != _lastChannelOption)
        {
            targetScript.SetChannelSplit(_channelOption);
            _lastChannelOption = _channelOption;
        }

        GUILayout.Space(20);


    }
}
