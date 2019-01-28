using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfoDataWidget : Selectable, IPointerClickHandler, ISubmitHandler {
    [Header("Info")]
    public Image iconImage;
    public bool iconResize;

    public TMPro.TMP_Text titleLabel;

    public GameObject newActiveGO;

    public InfoData data { get; private set; }

    public void Init(InfoData dat) {
        data = dat;

        if(newActiveGO) newActiveGO.SetActive(!data.isSeen);

        if(iconImage) {
            if(data.icon) {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = data.icon;
                if(iconResize)
                    iconImage.SetNativeSize();
            }
            else
                iconImage.gameObject.SetActive(false);
        }

        if(titleLabel) {
            var str = data.titleString;
            if(!string.IsNullOrEmpty(str)) {
                titleLabel.gameObject.SetActive(true);
                titleLabel.text = str;
            }
            else
                titleLabel.gameObject.SetActive(false);
        }
    }

    protected override void Awake() {
        base.Awake();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        OpenInfo();
    }

    void ISubmitHandler.OnSubmit(BaseEventData eventData) {
        OpenInfo();
    }

    void OpenInfo() {
        if(!interactable)
            return;

        if(!data)
            return;

        //open modal
        var parms = new M8.GenericParams();
        parms[ModalInfo.parmInfoData] = data;
        M8.ModalManager.main.Open(data.modal, parms);
    }
}
