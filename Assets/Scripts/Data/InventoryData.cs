using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "inventory", menuName = "Game/Inventory")]
public class InventoryData : ScriptableObject {
    [Header("Rocks")]
    public RockIgneousData[] rocksIgneous;
    public RockSedimentaryData[] rocksSedimentary;
    public RockMetamorphicData[] rocksMetamorphic;

    [Header("Others")]
    public MineralData[] minerals;
    public GemData[] gems;
    public OrganicData[] organics;

    public MagmaData magma;

    public int mineralsCount {
        get {
            int amt = 0;
            for(int i = 0; i < minerals.Length; i++)
                amt += minerals[i].count;
            return amt;
        }
    }

    public int rocksCount {
        get {
            int amt = 0;
            for(int i = 0; i < rocksIgneous.Length; i++)
                amt += rocksIgneous[i].count;
            for(int i = 0; i < rocksSedimentary.Length; i++)
                amt += rocksSedimentary[i].count;
            for(int i = 0; i < rocksMetamorphic.Length; i++)
                amt += rocksMetamorphic[i].count;
            return amt;
        }
    }

    public int organicsCount {
        get {
            int amt = 0;
            for(int i = 0; i < organics.Length; i++)
                amt += organics[i].count;
            return amt;
        }
    }

    public void ClearMineralsCount() {
        for(int i = 0; i < minerals.Length; i++)
            minerals[i].count = 0;
    }

    public void DeleteAll(bool deleteSeen) {
        for(int i = 0; i < rocksIgneous.Length; i++) {
            rocksIgneous[i].count = 0;

            if(deleteSeen)
                rocksIgneous[i].isSeen = false;
        }

        for(int i = 0; i < rocksSedimentary.Length; i++) {
            rocksSedimentary[i].count = 0;

            if(deleteSeen)
                rocksSedimentary[i].isSeen = false;
        }

        for(int i = 0; i < rocksMetamorphic.Length; i++) {
            rocksMetamorphic[i].count = 0;

            if(deleteSeen)
                rocksMetamorphic[i].isSeen = false;
        }

        for(int i = 0; i < minerals.Length; i++) {
            minerals[i].count = 0;

            if(deleteSeen)
                minerals[i].isSeen = false;
        }

        for(int i = 0; i < gems.Length; i++) {
            gems[i].count = 0;

            if(deleteSeen)
                gems[i].isSeen = false;
        }

        for(int i = 0; i < organics.Length; i++) {
            organics[i].count = 0;

            if(deleteSeen)
                organics[i].isSeen = false;
        }

        magma.count = 0;

        if(deleteSeen)
            magma.isSeen = false;

        M8.SceneState.instance.userData.Save();
    }
}
