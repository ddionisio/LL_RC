﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalInfo : M8.ModalController, M8.IModalActive, M8.IModalPush {
    public const string speechGroup = "description";

    public const string parmImage = "i";
    public const string parmTitleTextRef = "t";
    public const string parmDescTextRef = "d";

    [Header("Display")]
    public Image image;
    public bool isImageResize; //if true, set to correct size upon change

    public Text titleLabel;
    public Text descLabel;

    private LoLSpeakTextClick mTitleLabelSpeakText;
    private LoLSpeakTextClick mDescLabelSpeakText;

    private string mTitleTextRef;
    private string mDescTextRef;

    void Awake() {
        if(titleLabel) mTitleLabelSpeakText = titleLabel.GetComponent<LoLSpeakTextClick>();
        if(descLabel) mDescLabelSpeakText = descLabel.GetComponent<LoLSpeakTextClick>();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        if(image) {
            Sprite spr;
            if(parms.TryGetValue(parmImage, out spr) && spr) {
                image.gameObject.SetActive(true);
                image.sprite = spr;
                if(isImageResize)
                    image.SetNativeSize();
            }
            else
                image.gameObject.SetActive(false);
        }

        mTitleTextRef = parms.GetValue<string>(parmTitleTextRef);
        mDescTextRef = parms.GetValue<string>(parmDescTextRef);

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

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            //play speech
            if(!string.IsNullOrEmpty(mTitleTextRef))
                LoLManager.instance.SpeakTextQueue(mTitleTextRef, speechGroup, 0);

            if(!string.IsNullOrEmpty(mDescTextRef))
                LoLManager.instance.SpeakTextQueue(mDescTextRef, speechGroup, 1);
        }
    }
}
