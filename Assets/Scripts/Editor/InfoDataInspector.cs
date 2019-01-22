using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(InfoData), true)]
public class InfoDataInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            GUILayout.BeginVertical(GUI.skin.box);

            var dat = (InfoData)target;

            dat.count = EditorGUILayout.IntField("Count", dat.count);

            dat.isSeen = EditorGUILayout.Toggle("Is Seen", dat.isSeen);

            GUILayout.EndVertical();
        }
    }
}