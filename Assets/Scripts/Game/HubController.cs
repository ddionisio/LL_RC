using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

public class HubController : GameModeController<HubController> {

    public InventoryData inventory;

    protected override IEnumerator Start() {
        do {
            yield return null;
        } while(M8.SceneManager.instance.isLoading);

        //fail-safe
        var curProgress = LoLManager.instance.curProgress;
        switch(curProgress) {
            case 1:
            case 3:
            case 5:
                //add some minerals
                if(inventory.magma.count == 0 && inventory.mineralsCount == 0) {
                    for(int i = 0; i < inventory.minerals.Length; i++) {
                        if(inventory.minerals[i])
                            inventory.minerals[i].count += 1;
                    }
                }
                break;
            case 7:
                //add some organics
                if(inventory.rocksCount == 0 && inventory.organicsCount == 0) {
                    for(int i = 0; i < inventory.organics.Length; i++) {
                        if(inventory.organics[i])
                            inventory.organics[i].count += 5;
                    }
                }
                break;
            case 9:
                //add some minerals
                if(inventory.magma.count == 0 && inventory.mineralsCount == 0) {
                    for(int i = 0; i < inventory.minerals.Length; i++) {
                        if(inventory.minerals[i])
                            inventory.minerals[i].count += 1;
                    }
                }
                //add some organics
                /*if(inventory.rocksCount == 0 && inventory.organicsCount == 0) {
                    for(int i = 0; i < inventory.organics.Length; i++) {
                        if(inventory.organics[i])
                            inventory.organics[i].count += 5;
                    }
                }*/
                break;
        }

        if(signalModeChanged)
            signalModeChanged.Invoke(mode);
    }
}
