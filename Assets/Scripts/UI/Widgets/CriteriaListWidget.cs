using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriteriaListWidget : MonoBehaviour {
    public const int widgetCapacity = 8;

    public struct ItemData {
        public CriteriaWidget widget;
        public InfoData data;
    }

    public Transform root;
    public GameObject completeGO;

    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUpdate;

    public bool isComplete { get; private set; }
        
    private M8.CacheList<CriteriaWidget> mWidgets = new M8.CacheList<CriteriaWidget>(widgetCapacity);
    private M8.CacheList<CriteriaWidget> mWidgetCache = new M8.CacheList<CriteriaWidget>(widgetCapacity);

    private List<InfoData> mItems;

    public void UpdatePlay() {
        if(animator && !string.IsNullOrEmpty(takeUpdate))
            animator.Play(takeUpdate);
    }

    public void Refresh() {
        int widgetInd = 0;
        for(int i = 0; i < mItems.Count; i++) {
            if(widgetInd == mWidgets.Count)
                break;

            var item = mItems[i];
            if(item.count == 0)
                continue;

            mWidgets[widgetInd].SetUnlocked(item);
            widgetInd++;
        }

        //lock rest of widgets
        for(int i = widgetInd; i < mWidgets.Count; i++) {
            mWidgets[i].SetLocked();
            mWidgets[i].transform.SetAsLastSibling();
        }

        isComplete = widgetInd >= mWidgets.Count;

        if(completeGO)
            completeGO.SetActive(isComplete);
    }

    public void Init(CriteriaWidget template, int count, List<InfoData> dataList) {
        mItems = dataList;

        //clear previous
        if(mWidgets.Count > 0) {
            for(int i = 0; i < mWidgets.Count; i++) {
                var widget = mWidgets[i];
                widget.gameObject.SetActive(false);
                mWidgetCache.Add(widget);
            }

            mWidgets.Clear();
        }

        //add based on count
        for(int i = 0; i < count; i++) {
            CriteriaWidget newWidget;
            if(mWidgetCache.Count > 0) {
                newWidget = mWidgetCache.RemoveLast();
            }
            else {
                newWidget = Instantiate(template, root);
            }

            newWidget.gameObject.SetActive(true);

            mWidgets.Add(newWidget);
        }

        Refresh();
    }
}
