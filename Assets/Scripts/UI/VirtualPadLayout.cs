using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualPadLayout : MonoBehaviour {
    [Header("Display")]
    public GameObject rootGO;
    public RectTransform virtualPadTransform;
    public RectTransform virtualButtonTransform;

    [Header("Player")]
    [M8.TagSelector]
    public string playerTag;

    [Header("Signal")]
    public SignalVirtualPadLayoutType signalTypeChanged;

    private M8.ModalManager mMainModal;
    private PlayerController mPlayerCtrl;

    private VirtualPadLayoutType mCurType;
    private bool mIsVisible;

    void OnDestroy() {
        if(M8.SceneManager.isInstantiated)
            M8.SceneManager.instance.sceneChangePostCallback -= OnSceneChange;

        if(signalTypeChanged)
            signalTypeChanged.callback -= OnTypeChanged;
    }

    void Awake() {
        mIsVisible = false;

        if(rootGO) rootGO.SetActive(false);

        if(signalTypeChanged)
            signalTypeChanged.callback += OnTypeChanged;
    }

    void Start() {
        mMainModal = M8.ModalManager.main;

        RefreshPlayer();

        if(M8.SceneManager.isInstantiated)
            M8.SceneManager.instance.sceneChangePostCallback += OnSceneChange;
    }

    void OnSceneChange() {
        mIsVisible = false;

        RefreshPlayer();
        RefreshDisplay();
    }

    void Update() {
        if(mPlayerCtrl) {
            bool canShow = mPlayerCtrl.inputEnabled && mPlayerCtrl.bodyControl.enabled && (mMainModal == null || (mMainModal.activeCount == 0 && !mMainModal.isBusy));
            if(mIsVisible != canShow) {
                mIsVisible = canShow;
                RefreshDisplay();
            }
        }
    }

    void OnTypeChanged(VirtualPadLayoutType layout) {
        mCurType = layout;

        RefreshLayout();
        RefreshDisplay();
    }

    private void RefreshPlayer() {
        if(!mPlayerCtrl) {
            if(!string.IsNullOrEmpty(playerTag)) {
                var playerGO = GameObject.FindGameObjectWithTag(playerTag);
                if(playerGO)
                    mPlayerCtrl = playerGO.GetComponent<PlayerController>();
            }
        }
    }

    private void RefreshLayout() {
        switch(mCurType) {
            case VirtualPadLayoutType.Right:
                if(virtualPadTransform) {
                    virtualPadTransform.anchorMin = new Vector2 { x = 0f, y = virtualPadTransform.anchorMin.y };
                    virtualPadTransform.anchorMax = new Vector2 { x = 0f, y = virtualPadTransform.anchorMax.y };
                    virtualPadTransform.pivot = new Vector2 { x = 0f, y = virtualPadTransform.pivot.y };
                    virtualPadTransform.anchoredPosition = new Vector2 { x = 0f, y = virtualPadTransform.anchoredPosition.y };
                }

                if(virtualButtonTransform) {
                    virtualButtonTransform.anchorMin = new Vector2 { x = 1f, y = virtualButtonTransform.anchorMin.y };
                    virtualButtonTransform.anchorMax = new Vector2 { x = 1f, y = virtualButtonTransform.anchorMax.y };
                    virtualButtonTransform.pivot = new Vector2 { x = 1f, y = virtualButtonTransform.pivot.y };
                    virtualButtonTransform.anchoredPosition = new Vector2 { x = 0f, y = virtualButtonTransform.anchoredPosition.y };
                }
                break;

            case VirtualPadLayoutType.Left:
                if(virtualPadTransform) {
                    virtualPadTransform.anchorMin = new Vector2 { x = 1f, y = virtualPadTransform.anchorMin.y };
                    virtualPadTransform.anchorMax = new Vector2 { x = 1f, y = virtualPadTransform.anchorMax.y };
                    virtualPadTransform.pivot = new Vector2 { x = 1f, y = virtualPadTransform.pivot.y };
                    virtualPadTransform.anchoredPosition = new Vector2 { x = 0f, y = virtualPadTransform.anchoredPosition.y };
                }

                if(virtualButtonTransform) {
                    virtualButtonTransform.anchorMin = new Vector2 { x = 0f, y = virtualButtonTransform.anchorMin.y };
                    virtualButtonTransform.anchorMax = new Vector2 { x = 0f, y = virtualButtonTransform.anchorMax.y };
                    virtualButtonTransform.pivot = new Vector2 { x = 0f, y = virtualButtonTransform.pivot.y };
                    virtualButtonTransform.anchoredPosition = new Vector2 { x = 0f, y = virtualButtonTransform.anchoredPosition.y };
                }
                break;
        }
    }

    private void RefreshDisplay() {
        if(rootGO) rootGO.SetActive(mIsVisible && !(mCurType == VirtualPadLayoutType.None || mCurType == VirtualPadLayoutType.Keyboard));
    }
}
