using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubController : GameModeController<HubController> {

    public InventoryData inventory;

    protected override IEnumerator Start() {
        yield return base.Start();

        //fail-safe
        var curProgress = LoLManager.instance.curProgress;
        switch(curProgress) {
            case 1:
            case 3:
            case 5:
                //add some minerals
                if(inventory.mineralsCount == 0) {
                    for(int i = 0; i < inventory.minerals.Length; i++) {
                        if(inventory.minerals[i])
                            inventory.minerals[i].count += 1;
                    }
                }
                break;
            case 7:
                //add some organics
                if(inventory.organicsCount == 0) {
                    for(int i = 0; i < inventory.organics.Length; i++) {
                        if(inventory.organics[i])
                            inventory.organics[i].count += 5;
                    }
                }
                break;
            case 9:
                //add some minerals
                if(inventory.mineralsCount == 0) {
                    for(int i = 0; i < inventory.minerals.Length; i++) {
                        if(inventory.minerals[i])
                            inventory.minerals[i].count += 1;
                    }
                }
                //add some organics
                if(inventory.organicsCount == 0) {
                    for(int i = 0; i < inventory.organics.Length; i++) {
                        if(inventory.organics[i])
                            inventory.organics[i].count += 5;
                    }
                }
                break;
        }
    }
}
