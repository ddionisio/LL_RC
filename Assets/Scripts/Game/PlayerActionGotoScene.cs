using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to a trigger
/// </summary>
public class PlayerActionGotoScene : PlayerAction {
    public M8.SceneAssetPath scene;

    public bool saveStartCheckpoint;

    public override void ActionInvoke(PlayerController player) {
        player.stateControl.state = player.stateDespawn;

        if(saveStartCheckpoint)
            Checkpoint.SetStart(player.transform);

        scene.Load();
    }
}
