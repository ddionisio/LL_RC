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

    public void ClearMineralsCount() {
        for(int i = 0; i < minerals.Length; i++)
            minerals[i].count = 0;
    }

    public void ClearAllCounts() {
        for(int i = 0; i < rocksIgneous.Length; i++)
            rocksIgneous[i].count = 0;

        for(int i = 0; i < rocksSedimentary.Length; i++)
            rocksSedimentary[i].count = 0;

        for(int i = 0; i < rocksMetamorphic.Length; i++)
            rocksMetamorphic[i].count = 0;

        for(int i = 0; i < minerals.Length; i++)
            minerals[i].count = 0;

        for(int i = 0; i < gems.Length; i++)
            gems[i].count = 0;

        for(int i = 0; i < organics.Length; i++)
            organics[i].count = 0;

        magma.count = 0;

        M8.SceneState.instance.userData.Save();
    }
}
