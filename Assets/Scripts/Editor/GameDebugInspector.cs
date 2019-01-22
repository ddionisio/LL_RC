using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GameDebug))]
public class GameDebugInspector : Editor {

    private int mProgressInput;

    void OnEnable() {
        if(Application.isPlaying) {
            mProgressInput = LoLManager.instance.curProgress;
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            GUILayout.BeginVertical(GUI.skin.box);

            var dat = (GameDebug)target;

            GUILayout.Label("Current Progress: " + LoLManager.instance.curProgress);

            GUILayout.BeginHorizontal();
            mProgressInput = EditorGUILayout.IntField("Progress", mProgressInput);
            if(GUILayout.Button("Apply", GUILayout.Width(60f))) {
                LoLManager.instance.ApplyProgress(mProgressInput);
                dat.signalProgressChanged.Invoke();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
    }
}
