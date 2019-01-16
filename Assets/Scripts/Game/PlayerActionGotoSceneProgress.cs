using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to a trigger
/// </summary>
public class PlayerActionGotoSceneProgress : PlayerAction {
    [System.Serializable]
    public struct Info {
        public int progress;
        public M8.SceneAssetPath scene;
    }

    public Info[] infos;

    public override void ActionInvoke() {
        if(M8.SceneManager.instance.isLoading)
            return;

        int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

        for(int i = 0; i < infos.Length; i++) {
            var inf = infos[i];

            if(inf.progress == progress) {
                inf.scene.Load();
                break;
            }
        }
    }
}
