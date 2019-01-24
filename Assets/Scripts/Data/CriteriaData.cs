using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "criteriaData", menuName = "Game/Criteria")]
public class CriteriaData : M8.SingletonScriptableObject<CriteriaData> {
    [System.Serializable]
    public struct CriteriaItem {
        public int progress; //which progress this criteria is for
        public int igneousCount;
        public int sedimentaryCount;
        public int metamorphicCount;
    }

    public CriteriaItem[] items;

    [Header("Signals")]
    public M8.Signal signalUpdateIgneous;
    public M8.Signal signalUpdateSedimentary;
    public M8.Signal signalUpdateMetamorphic;

    public bool IsComplete(InventoryData inventory) {
        int igneousCount, sedimentaryCount, metamorphicCount;
        GetCounts(out igneousCount, out sedimentaryCount, out metamorphicCount);

        if(igneousCount == 0 && sedimentaryCount == 0 && metamorphicCount == 0)
            return false;

        bool isIgneousComplete, isSedimentaryComplete, isMetamorphicComplete;

        if(igneousCount > 0) {
            int count = 0;
            for(int i = 0; i < inventory.rocksIgneous.Length; i++) {
                if(inventory.rocksIgneous[i].count > 0)
                    count++;
            }

            isIgneousComplete = count >= igneousCount;
        }
        else
            isIgneousComplete = true;

        if(sedimentaryCount > 0) {
            int count = 0;
            for(int i = 0; i < inventory.rocksSedimentary.Length; i++) {
                if(inventory.rocksSedimentary[i].count > 0)
                    count++;
            }

            isSedimentaryComplete = count >= sedimentaryCount;
        }
        else
            isSedimentaryComplete = true;

        if(metamorphicCount > 0) {
            int count = 0;
            for(int i = 0; i < inventory.rocksMetamorphic.Length; i++) {
                if(inventory.rocksMetamorphic[i].count > 0)
                    count++;
            }

            isMetamorphicComplete = count >= metamorphicCount;
        }
        else
            isMetamorphicComplete = true;

        return isIgneousComplete && isSedimentaryComplete && isMetamorphicComplete;
    }

    public void GetCounts(out int igneousCount, out int sedimentaryCount, out int metamorphicCount) {
        int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

        igneousCount = 0; sedimentaryCount = 0; metamorphicCount = 0;

        for(int i = 0; i < items.Length; i++) {
            var criteriaItem = items[i];

            if(criteriaItem.progress == progress) {
                igneousCount = criteriaItem.igneousCount;
                sedimentaryCount = criteriaItem.sedimentaryCount;
                metamorphicCount = criteriaItem.metamorphicCount;
                break;
            }
        }
    }
}
