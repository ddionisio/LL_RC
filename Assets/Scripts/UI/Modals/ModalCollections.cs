using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalCollections : M8.ModalController, M8.IModalPush, M8.IModalActive {
    [System.Serializable]
    public class RockListData {
        public Transform itemRoot;

        public CollectionItemWidget[] items { get; private set; }
        
        public void Init(CollectionItemWidget template, RockData[] rocks) {
            items = new CollectionItemWidget[rocks.Length];

            for(int i = 0; i < rocks.Length; i++) {
                var newItem = Instantiate(template);
                newItem.transform.SetParent(itemRoot, false);
                newItem.gameObject.SetActive(true);

                newItem.Init(rocks[i]);

                items[i] = newItem;
            }
        }

        public void RefreshDisplay() {
            for(int i = 0; i < items.Length; i++) {
                var item = items[i];
                item.RefreshDisplay();
            }

            //re-arrange based on seen status

            System.Array.Sort(items, delegate (CollectionItemWidget a, CollectionItemWidget b) {
                var rockData1 = a.rockData;
                var rockData2 = b.rockData;

                //order is: seen, newly seen, unseen

                if(rockData1.isSeen && rockData2.isSeen) {
                    if(!rockData1.isNewlySeen)
                        return -1;
                    else if(!rockData2.isNewlySeen)
                        return 1;
                }
                else if(rockData1.isSeen)
                    return -1;
                else if(rockData2.isSeen)
                    return 1;
                
                return 0;
            });

            for(int i = 0; i < items.Length; i++) {
                var item = items[i];
                item.transform.SetSiblingIndex(i);
            }
        }
    }

    [Header("Data")]
    public InventoryData inventory;
    public CollectionItemWidget itemTemplate;

    [Header("Rock Lists")]
    public RockListData igneousList;
    public RockListData sedimentaryList;
    public RockListData metamorphicList;

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //initialize
        if(igneousList.items == null) {
            var rocks = inventory.rocksIgneous;
            igneousList.Init(itemTemplate, rocks);
        }

        if(sedimentaryList.items == null) {
            var rocks = inventory.rocksSedimentary;
            sedimentaryList.Init(itemTemplate, rocks);
        }

        if(metamorphicList.items == null) {
            var rocks = inventory.rocksMetamorphic;
            metamorphicList.Init(itemTemplate, rocks);
        }

        //refresh display
        igneousList.RefreshDisplay();
        sedimentaryList.RefreshDisplay();
        metamorphicList.RefreshDisplay();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            StartCoroutine(DoReveals());
        }
    }

    void Awake() {
        itemTemplate.gameObject.SetActive(false);
    }

    IEnumerator DoReveals() {
        //go through reveals one by one, and untag 'newlySeen' for rockData

        for(int i = 0; i < igneousList.items.Length; i++) {
            var item = igneousList.items[i];

            if(item.rockData.isNewlySeen) {
                item.rockData.ClearNewlySeen();

                yield return item.PlayNewlySeen();
            }
        }

        for(int i = 0; i < sedimentaryList.items.Length; i++) {
            var item = sedimentaryList.items[i];

            if(item.rockData.isNewlySeen) {
                item.rockData.ClearNewlySeen();

                yield return item.PlayNewlySeen();
            }
        }

        for(int i = 0; i < metamorphicList.items.Length; i++) {
            var item = metamorphicList.items[i];

            if(item.rockData.isNewlySeen) {
                item.rockData.ClearNewlySeen();

                yield return item.PlayNewlySeen();
            }
        }
    }
}
