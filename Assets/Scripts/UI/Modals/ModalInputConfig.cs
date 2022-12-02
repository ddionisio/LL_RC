using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalInputConfig : M8.ModalController, M8.IModalPush, M8.IModalPop {
    [System.Serializable]
    public struct ItemData {
        public int index;
        public Button button;
        public GameObject selectGO;

        public void AddClickListener(UnityEngine.Events.UnityAction call) {
            button.onClick.AddListener(call);
        }

        public void SetSelect(bool selected) {
            if(selectGO) selectGO.SetActive(selected);
        }
    }

    [Header("UI")]
    public ItemData[] items;
    public Selectable closeButton;
    public Selectable disableOnPush;

    [Header("Data")]
    public M8.UserData settingsData;

    [Header("Signal")]
    public SignalVirtualPadLayoutType signalTypeChanged;

    private int mCurInputTypeIndex;
    private bool mIsChanged;

    void M8.IModalPop.Pop() {
        if(mIsChanged && settingsData) {
            settingsData.SetInt(GlobalSettings.inputUserKey, mCurInputTypeIndex);
            settingsData.Save();
        }

        if(disableOnPush)
            disableOnPush.interactable = true;
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        if(disableOnPush)
            disableOnPush.interactable = false;

        if(settingsData)
            mCurInputTypeIndex = settingsData.GetInt(GlobalSettings.inputUserKey);
        else
            mCurInputTypeIndex = 0;

        RefreshSelect();
    }

    void Awake() {
        for(int i = 0; i < items.Length; i++) {
            var itm = items[i];
            int index = itm.index;

            itm.AddClickListener(delegate () { OnSelectItem(index); });
            itm.SetSelect(false);
        }
    }

    void OnSelectItem(int index) {
        if(mCurInputTypeIndex != index) {
            mCurInputTypeIndex = index;
            mIsChanged = true;

            RefreshSelect();

            if(signalTypeChanged) signalTypeChanged.Invoke((VirtualPadLayoutType)index);
        }
    }

    void RefreshSelect() {
        for(int i = 0; i < items.Length; i++) {
            items[i].SetSelect(items[i].index == mCurInputTypeIndex);
        }

        if(closeButton)
            closeButton.interactable = mCurInputTypeIndex > 0;
    }
}
