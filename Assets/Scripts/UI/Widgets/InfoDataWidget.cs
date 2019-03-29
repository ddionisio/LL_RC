using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfoDataWidget : Selectable, IPointerClickHandler, ISubmitHandler {
    [Header("Info")]
    public Image iconImage;
    public RawImage iconRawImage;

    public Text titleLabel;

    public GameObject newActiveGO;

    public InfoData data { get; private set; }

    public void Init(InfoData dat) {
        data = dat;

        if(newActiveGO) newActiveGO.SetActive(!data.isSeen);

        if(data.icon) {
            iconImage.gameObject.SetActive(true);
            iconRawImage.gameObject.SetActive(false);

            iconImage.sprite = data.icon;
            iconImage.SetNativeSize();
        }
        else {
            //TODO: only rock data for now
            var rockData = dat as RockData;
            if(rockData && rockData.spriteShape) {
                iconImage.gameObject.SetActive(false);
                iconRawImage.gameObject.SetActive(true);

                iconRawImage.texture = rockData.spriteShape.fillTexture;
            }
            else {
                iconImage.gameObject.SetActive(false);
                iconRawImage.gameObject.SetActive(false);
            }
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
