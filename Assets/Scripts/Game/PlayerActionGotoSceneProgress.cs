using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

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

    public bool saveStartCheckpoint;

    public override void ActionInvoke(PlayerController player) {
        player.stateControl.state = player.stateDespawn;

        if(saveStartCheckpoint)
            Checkpoint.SetStart(player.transform);

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
