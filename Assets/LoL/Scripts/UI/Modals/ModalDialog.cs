﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LoLExt;

public class ModalDialog : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
    public const string modalNameGeneric = "dialog";

    public const string parmPortraitSprite = "p";
    public const string parmNameTextRef = "n";
    public const string parmDialogTextRef = "t";
    public const string parmNextCallback = "c";

    [Header("Display")]
    public Image portraitImage;
    public bool portraitResize;

    public Text nameLabel;

    public Text textLabel;
    public float textCharPerSecond = 0.1f;

    public GameObject textProcessActiveGO;
    public GameObject textProcessFinishGO;

    [Header("Data")]
    public float nextDelay = 1f; //when we are allowed to process next since active
    public bool isCloseOnNext;
    public bool isTextSpeechAuto = true;

    [Header("Signals")]
    public M8.Signal signalNext; //when the next button is pressed.
    
    private static M8.GenericParams mParms = new M8.GenericParams();

    private string mDialogTextRef;
    private System.Action mNextCallback;

    private Coroutine mTextProcessRout;

    private System.Text.StringBuilder mTextProcessSB = new System.Text.StringBuilder();
    private string mTextDialog;

    private bool mIsActive;
    private float mLastActiveTime;

    private bool mIsNextProcessed;

    /// <summary>
    /// Open generic dialog: modalNameGeneric
    /// </summary>
    public static void Open(string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        Open(modalNameGeneric, nameTextRef, dialogTextRef, nextCallback);
    }

    public static void Open(string modalName, string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        //check to see if there's one already opened
        var uiMgr = M8.ModalManager.main;
                
        if(uiMgr.IsInStack(modalName)) {
            var dlg = uiMgr.GetBehaviour<ModalDialog>(modalName);

            dlg.mNextCallback = nextCallback;

            dlg.SetupTextContent(nameTextRef, dialogTextRef);

            dlg.ApplyActive();
        }
        else {
            mParms[parmNameTextRef] = nameTextRef;
            mParms[parmDialogTextRef] = dialogTextRef;
            mParms[parmNextCallback] = nextCallback;

            uiMgr.Open(modalName, mParms);
        }
    }

    /// <summary>
    /// Open generic dialog, and apply portrait: modalNameGeneric. set portrait to null to hide portrait
    /// </summary>
    public static void OpenApplyPortrait(Sprite portrait, string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        OpenApplyPortrait(modalNameGeneric, portrait, nameTextRef, dialogTextRef, nextCallback);
    }

    /// <summary>
    /// Open dialog and apply portrait. set portrait to null to hide portrait
    /// </summary>
    public static void OpenApplyPortrait(string modalName, Sprite portrait, string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        //check to see if there's one already opened
        var uiMgr = M8.ModalManager.main;

        if(uiMgr.IsInStack(modalName)) {
            var dlg = uiMgr.GetBehaviour<ModalDialog>(modalName);

            dlg.mNextCallback = nextCallback;

            dlg.SetupPortraitTextContent(portrait, nameTextRef, dialogTextRef);

            dlg.ApplyActive();
        }
        else {
            mParms[parmPortraitSprite] = portrait;
            mParms[parmNameTextRef] = nameTextRef;
            mParms[parmDialogTextRef] = dialogTextRef;
            mParms[parmNextCallback] = nextCallback;

            uiMgr.Open(modalName, mParms);
        }
    }

    /// <summary>
    /// Close generic modal dialog
    /// </summary>
    public static void CloseGeneric() {
        var uiMgr = M8.ModalManager.main;

        if(uiMgr.IsInStack(modalNameGeneric)) {
            uiMgr.CloseUpTo(modalNameGeneric, true);
        }
    }

    public void Next() {
        if(mIsNextProcessed)
            return;

        if(!mIsActive)
            return;
        
        if(mTextProcessRout != null) { //finish up text process, need to click next one more time
            if(Time.time - mLastActiveTime < nextDelay)
                return;

            StopTextProcess();
            textLabel.text = mTextDialog;

            if(textProcessFinishGO) textProcessFinishGO.SetActive(true);

            return;
        }

        mIsNextProcessed = true;

        var nextCB = mNextCallback;
        mNextCallback = null;

        if(isCloseOnNext)
            Close();

        if(nextCB != null)
            nextCB();

        if(signalNext != null)
            signalNext.Invoke();
    }

    public void PlayDialogSpeech() {
        if(LoLManager.isInstantiated && !string.IsNullOrEmpty(mDialogTextRef))
            LoLManager.instance.SpeakText(mDialogTextRef);
    }
        
    void M8.IModalActive.SetActive(bool aActive) {
        mIsActive = aActive;

        ApplyActive();
    }

    private void ApplyActive() {
        if(mIsActive) {
            mIsNextProcessed = false;

            mLastActiveTime = Time.time;

            //play text speech if auto
            if(isTextSpeechAuto)
                PlayDialogSpeech();

            PlayTextProcess();
        }   
    }

    void M8.IModalPop.Pop() {
        mIsActive = false;

        StopTextProcess();

        if(textProcessFinishGO) textProcessFinishGO.SetActive(false);

        mNextCallback = null;
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        if(parms != null) {
            if(parms.ContainsKey(parmPortraitSprite))
                SetupPortraitTextContent(parms.GetValue<Sprite>(parmPortraitSprite), parms.GetValue<string>(parmNameTextRef), parms.GetValue<string>(parmDialogTextRef));
            else
                SetupTextContent(parms.GetValue<string>(parmNameTextRef), parms.GetValue<string>(parmDialogTextRef));

            mNextCallback = parms.GetValue<System.Action>(parmNextCallback);
        }

        if(textProcessActiveGO) textProcessActiveGO.SetActive(false);
		if(textProcessFinishGO) textProcessFinishGO.SetActive(false);

        mIsNextProcessed = false;
    }

    private void SetupPortraitTextContent(Sprite portrait, string nameTextRef, string dialogTextRef) {        
        if(portraitImage) {
            if(portrait) {
                portraitImage.gameObject.SetActive(true);

                portraitImage.sprite = portrait;
                if(portraitResize)
                    portraitImage.SetNativeSize();
            }
            else
                portraitImage.gameObject.SetActive(false);
        }

        SetupTextContent(nameTextRef, dialogTextRef);
    }

    private void SetupTextContent(string nameTextRef, string dialogTextRef) {
        StopTextProcess();

        if(textProcessFinishGO) textProcessFinishGO.SetActive(false);

        //setup other stuff?

        mDialogTextRef = dialogTextRef;

        if(nameLabel) {
            nameLabel.text = !string.IsNullOrEmpty(nameTextRef) ? M8.Localize.Get(nameTextRef) : "";
        }

        textLabel.text = "";

        mTextDialog = !string.IsNullOrEmpty(dialogTextRef) ? M8.Localize.Get(dialogTextRef) : "";
    }

    private void PlayTextProcess() {
        StopTextProcess();
        mTextProcessRout = StartCoroutine(DoTextProcess());
    }

    private void StopTextProcess() {
        if(mTextProcessRout != null) {
            StopCoroutine(mTextProcessRout);
            mTextProcessRout = null;
        }
    }

    IEnumerator DoTextProcess() {
        if(textProcessActiveGO) textProcessActiveGO.SetActive(true);
		if(textProcessFinishGO) textProcessFinishGO.SetActive(false);

        mTextProcessSB.Clear();

        var waitDelay = new WaitForSeconds(textCharPerSecond);

        for(int i = 0; i < mTextDialog.Length; i++) {
            mTextProcessSB.Append(mTextDialog[i]);

            textLabel.text = mTextProcessSB.ToString();

            yield return waitDelay;
        }

        mTextProcessRout = null;

		if(textProcessActiveGO) textProcessActiveGO.SetActive(false);
		if(textProcessFinishGO) textProcessFinishGO.SetActive(true);
    }
}