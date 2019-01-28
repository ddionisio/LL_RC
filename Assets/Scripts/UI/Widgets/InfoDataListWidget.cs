using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoDataListWidget : MonoBehaviour {
    public const int itemCapacity = 16;

    [Header("Info")]
    public InfoDataWidget template;
    public bool navigationAuto = false;

    private M8.CacheList<InfoDataWidget> mItemActives = new M8.CacheList<InfoDataWidget>(itemCapacity);
    private M8.CacheList<InfoDataWidget> mItemCache = new M8.CacheList<InfoDataWidget>(itemCapacity);

    /// <summary>
    /// downSelect is optional for navigation
    /// </summary>
    public void Init(List<InfoData> data, Selectable downSelect) {
        //clear out actives
        for(int i = 0; i < mItemActives.Count; i++) {
            var item = mItemActives[i];
            if(item) {
                item.gameObject.SetActive(false);
                mItemCache.Add(item);
            }
        }

        mItemActives.Clear();
        //

        //add items
        InfoDataWidget prevItem = null;

        for(int i = 0; i < data.Count; i++) {
            InfoDataWidget newItem = null;

            if(mItemCache.Count > 0) {
                newItem = mItemCache.RemoveLast();
                if(newItem)
                    newItem.gameObject.SetActive(true);
            }
            else if(!mItemActives.IsFull) {
                newItem = Instantiate(template);
                newItem.transform.SetParent(template.transform.parent, false);
                newItem.gameObject.SetActive(true);
            }

            if(!newItem)
                continue;

            newItem.Init(data[i]);

            if(navigationAuto) {
                var nav = newItem.navigation;
                nav.mode = Navigation.Mode.Automatic;
                newItem.navigation = nav;
            }
            else {
                if(prevItem) {
                    var prevNav = prevItem.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    prevNav.selectOnRight = newItem;
                    prevItem.navigation = prevNav;
                }

                var nav = newItem.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnLeft = prevItem;
                nav.selectOnDown = downSelect;
                newItem.navigation = nav;
            }

            mItemActives.Add(newItem);
        }
    }

    void Awake() {
        if(template)
            template.gameObject.SetActive(false);
    }
}
