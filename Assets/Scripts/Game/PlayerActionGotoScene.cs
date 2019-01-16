using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to a trigger
/// </summary>
public class PlayerActionGotoScene : PlayerAction {
    public M8.SceneAssetPath scene;

    public override void ActionInvoke() {
        if(M8.SceneManager.instance.isLoading)
            return;

        scene.Load();
    }
}
