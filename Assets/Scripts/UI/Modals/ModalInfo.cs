using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using LoLExt;

public class ModalInfo : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
    public const string speechGroup = "description";

    public const string parmInfoData = "infoData";

    [Header("Info Data")]
    public Image imageIcon;
    public bool isImageIconResize; //if true, set to correct size upon change
    public Image imageIllustrate;
    public bool isImageIllustrateResize; //if true, set to correct size upon change

    public Text titleLabel;
    public Text descLabel;

    public Selectable selectOnActive;

    public InfoData infoData { get; private set; }

    private LoLSpeakTextClick mTitleLabelSpeakText;
    private LoLSpeakTextClick mDescLabelSpeakText;

    private string mTitleTextRef;
    private string mDescTextRef;

    void Awake() {
        if(titleLabel) mTitleLabelSpeakText = titleLabel.GetComponent<LoLSpeakTextClick>();
        if(descLabel) mDescLabelSpeakText = descLabel.GetComponent<LoLSpeakTextClick>();
    }

    public virtual void Pop() {
        infoData = null;
    }

    public virtual void Push(M8.GenericParams parms) {
        if(parms != null && parms.ContainsKey(parmInfoData))
            infoData = parms.GetValue<InfoData>(parmInfoData);
        else
            infoData = null;

        ApplyInfoData();
    }

    public virtual void SetActive(bool aActive) {
        if(aActive) {
            if(selectOnActive)
                selectOnActive.Select();

            //play speech
            if(!string.IsNullOrEmpty(mTitleTextRef))
                LoLManager.instance.SpeakTextQueue(mTitleTextRef, speechGroup, 0);

            if(!string.IsNullOrEmpty(mDescTextRef))
                LoLManager.instance.SpeakTextQueue(mDescTextRef, speechGroup, 1);
        }
    }

    protected virtual void ApplyInfoData() {
        if(infoData) {
            infoData.isSeen = true; //tag as seen

            if(imageIcon) {
                if(infoData.icon) {
                    imageIcon.gameObject.SetActive(true);
                    imageIcon.sprite = infoData.icon;
                    if(isImageIconResize)
                        imageIcon.SetNativeSize();
                }
                else
                    imageIcon.gameObject.SetActive(false);
            }

            if(imageIllustrate) {
                if(infoData.illustration) {
                    imageIllustrate.gameObject.SetActive(true);
                    imageIllustrate.sprite = infoData.illustration;
                    if(isImageIllustrateResize)
                        imageIllustrate.SetNativeSize();
                }
                else
                    imageIllustrate.gameObject.SetActive(false);
            }

            mTitleTextRef = infoData.titleRef;
            mDescTextRef = infoData.descRef;

            if(titleLabel) {
                if(!string.IsNullOrEmpty(mTitleTextRef)) {
                    titleLabel.gameObject.SetActive(true);
                    titleLabel.text = M8.Localize.Get(mTitleTextRef);
                }
                else
                    titleLabel.gameObject.SetActive(false);
            }

            if(descLabel) {
                if(!string.IsNullOrEmpty(mDescTextRef)) {
                    descLabel.gameObject.SetActive(true);
                    descLabel.text = M8.Localize.Get(mDescTextRef);
                }
                else
                    descLabel.gameObject.SetActive(false);
            }

            if(mTitleLabelSpeakText) mTitleLabelSpeakText.key = mTitleTextRef;
            if(mDescLabelSpeakText) mDescLabelSpeakText.key = mDescTextRef;
        }
    }
}
