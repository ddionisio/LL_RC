using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    public InventoryData inventory;
    public M8.SceneAssetPath toScene;

    private MineralData[] mNewMinerals;

    public void Proceed() {
        //
        if(LoLManager.isInstantiated)
            LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

        toScene.Load();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //setup display of minerals and gems
        //determine which ones are not seen to determine which ones to show
        var mineralList = new List<MineralData>();
        for(int i = 0; i < inventory.minerals.Length; i++) {
            var mineralDat = inventory.minerals[i];
            if(mineralDat.count > 0 && !mineralDat.isSeen) {
                mineralDat.isSeen = true;
                mineralList.Add(mineralDat);
            }
        }

        mNewMinerals = mineralList.ToArray();
    }
}
