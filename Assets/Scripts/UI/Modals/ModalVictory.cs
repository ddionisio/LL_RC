using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    [Header("Data")]
    public M8.SceneAssetPath toScene;

    [Header("UI")]
    public InfoDataListWidget infoList;

    public void Proceed() {
        //
        if(LoLManager.isInstantiated)
            LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

        toScene.Load();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //apply outcome
        CollectController.instance.ApplyOutcome();

        //setup display of collections
        var collects = CollectController.instance.collectList;
        infoList.Init(collects, null);
    }
}
