using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to a trigger
/// </summary>
public class PlayerActionGotoSceneProgressSingle : PlayerAction {
    
    public int[] progressMatch;
    public M8.SceneAssetPath scene;

    public bool saveStartCheckpoint;

    public override void ActionInvoke(PlayerController player) {
        player.stateControl.state = player.stateDespawn;

        if(saveStartCheckpoint)
            Checkpoint.SetStart(player.transform);

        int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

        for(int i = 0; i < progressMatch.Length; i++) {
            if(progress == progressMatch[i]) {
                scene.Load();
                break;
            }
        }
    }
}
