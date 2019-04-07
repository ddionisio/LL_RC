using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHub : MonoBehaviour {
    [System.Serializable]
    public struct CriteriaItem {
        public int progress; //which progress this criteria is for
        public int igneousCount;
        public int sedimentaryCount;
        public int metamorphicCount;
    }

    public InventoryData inventory;
    public CriteriaData criteria;

    [Header("Template")]
    public CriteriaWidget template;

    [Header("Igneous")]
    public CriteriaListWidget criteraIgneous;
    public bool criteriaIgneousEnabled = true;

    [Header("Sedimentary")]
    public CriteriaListWidget criteraSedimentary;
    public bool criteriaSedimentaryEnabled = true;

    [Header("Metamorphic")]
    public CriteriaListWidget criteraMetamorphic;
    public bool criteriaMetamorphicEnabled = true;

    [Header("Signal")]
    public M8.Signal signalHide;
    public M8.Signal signalProgressUpdate;

    void Awake() {
        template.gameObject.SetActive(false);
    }

    void OnDisable() {
        criteria.signalUpdateIgneous.callback -= OnUpdateIgneous;
        criteria.signalUpdateSedimentary.callback -= OnUpdateSedimentary;
        criteria.signalUpdateMetamorphic.callback -= OnUpdateMetamorphic;

        signalHide.callback -= OnHide;
        signalProgressUpdate.callback -= OnUpdateProgress;
    }

    void OnEnable() {
        criteria.signalUpdateIgneous.callback += OnUpdateIgneous;
        criteria.signalUpdateSedimentary.callback += OnUpdateSedimentary;
        criteria.signalUpdateMetamorphic.callback += OnUpdateMetamorphic;

        signalHide.callback += OnHide;
        signalProgressUpdate.callback += OnUpdateProgress;

        OnUpdateProgress();
    }

    void OnHide() {
        criteraIgneous.gameObject.SetActive(false);
        criteraSedimentary.gameObject.SetActive(false);
        criteraMetamorphic.gameObject.SetActive(false);
    }

    void OnUpdateProgress() {
        int igneousCount, sedimentaryCount, metamorphicCount;
        criteria.GetCounts(out igneousCount, out sedimentaryCount, out metamorphicCount);

        if(igneousCount > 0 && criteriaIgneousEnabled) {
            criteraIgneous.gameObject.SetActive(true);
            criteraIgneous.Init(template, igneousCount, new List<InfoData>(inventory.rocksIgneous));
        }
        else
            criteraIgneous.gameObject.SetActive(false);

        if(sedimentaryCount > 0 && criteriaSedimentaryEnabled) {
            criteraSedimentary.gameObject.SetActive(true);
            criteraSedimentary.Init(template, sedimentaryCount, new List<InfoData>(inventory.rocksSedimentary));
        }
        else
            criteraSedimentary.gameObject.SetActive(false);

        if(metamorphicCount > 0 && criteriaMetamorphicEnabled) {
            criteraMetamorphic.gameObject.SetActive(true);
            criteraMetamorphic.Init(template, metamorphicCount, new List<InfoData>(inventory.rocksMetamorphic));
        }
        else
            criteraMetamorphic.gameObject.SetActive(false);
    }

    void OnUpdateIgneous() {
        if(criteraIgneous.gameObject.activeSelf) {
            if(criteraIgneous.Refresh())
                criteraIgneous.UpdatePlay();
        }
    }

    void OnUpdateSedimentary() {
        if(criteraSedimentary.gameObject.activeSelf) {
            if(criteraSedimentary.Refresh())
                criteraSedimentary.UpdatePlay();
        }
    }

    void OnUpdateMetamorphic() {
        if(criteraMetamorphic.gameObject.activeSelf) {
            if(criteraMetamorphic.Refresh())
                criteraMetamorphic.UpdatePlay();
        }
    }
}
