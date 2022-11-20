using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(CollectController))]
public class CollectControllerInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            GUILayout.BeginVertical(GUI.skin.box);

            var dat = (CollectController)target;

            if(GUILayout.Button("Collect Fill")) {
                dat.signalCollect.Invoke(dat.collectMaxCount);
            }

            GUILayout.EndVertical();
        }
    }
}
