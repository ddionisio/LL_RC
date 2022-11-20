using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

/// <summary>
/// Add this to a trigger
/// </summary>
public class PlayerActionGotoSceneProgressSingle : PlayerAction {
    
    public int[] progressMatch;
    public M8.SceneAssetPath scene;
    [M8.SoundPlaylist]
    public string soundFX;

    public bool saveStartCheckpoint;

    public override void ActionInvoke(PlayerController player) {
        if(!string.IsNullOrEmpty(soundFX))
            M8.SoundPlaylist.instance.Play(soundFX, false);

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
