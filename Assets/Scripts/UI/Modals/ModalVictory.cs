using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    public M8.SceneAssetPath toScene;

    private List<InfoData> mMineralList;

    public void Proceed() {
        //
        if(LoLManager.isInstantiated)
            LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

        toScene.Load();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //setup display of minerals and gems
        mMineralList = new List<InfoData>();

        var collectList = CollectController.instance.collectList;
        for(int i = 0; i < collectList.Count; i++) {
            var item = collectList[i];

            if(item is MineralData)
                mMineralList.Add(item);
        }

        
    }
}
