using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RockSelectWidget : Selectable {
    public enum State {
        None,
        Selecting,
        Processing,
        Proceed
    }

    [Header("UI")]
    public GameObject selectActiveGO;
    public GameObject disableGO;

    [Header("Inventory")]
    public InventoryData inventory;

    [Header("Input")]
    public M8.InputAction processInput; //use the accept input action
    public M8.InputAction selectAxisInput; //axis input to select prev/next (-1 = left/down, 1 = right/up)

    [Header("Rock")]
    public Text rockTitleText;
    public Text rockCountText;
    public string rockCountFormat = "00";

    public float rockProcessDelay = 1f; //delay before processing while pointer is down
    public int rockDisplayCount = 3;
    public Transform rockRotate;
    public RockSelectItemWidget[] rockItems;
    public float rockRotateAmount;
    public float rockRotateDelay = 0.3f;
        
    public bool isProcess { get; private set; } //pointer remains down after delay

    private bool mIsSelected;
    private bool mIsDown;

    public int mCurRockInd;

    public event System.Action<InfoData> processRockCallback;

    private List<InfoData> mRockList = new List<InfoData>();

    private State mCurState = State.None;
    private float mCurTime;
    private bool mIsRotateLeft;

    private bool mIsInteractible;

    private DG.Tweening.EaseFunction mRotateTweenFunc;

    public void RefreshRock(InfoData dat) {
        if(dat.count == 0) {
            if(mRockList.Remove(dat)) {
                if(mCurRockInd >= mRockList.Count)
                    mCurRockInd = 0;

                UpdateRockDisplay();
            }
        }
        else if(mRockList[mCurRockInd] == dat) {
            //update count
            UpdateCurrentRockCountDisplay();
        }
    }

    public void Refresh(bool resetIndex) {
        ClearRocks();

        //add rocks with count > 0
        for(int i = 0; i < inventory.rocksIgneous.Length; i++) {
            var rock = inventory.rocksIgneous[i];
            if(rock.count > 0)
                mRockList.Add(rock);
        }

        for(int i = 0; i < inventory.rocksSedimentary.Length; i++) {
            var rock = inventory.rocksSedimentary[i];
            if(rock.count > 0)
                mRockList.Add(rock);
        }

        for(int i = 0; i < inventory.rocksMetamorphic.Length; i++) {
            var rock = inventory.rocksMetamorphic[i];
            if(rock.count > 0)
                mRockList.Add(rock);
        }

        if(resetIndex) {
            if(mRockList.Count == 3)
                mCurRockInd = 1;
            else
                mCurRockInd = 0;
        }
        else
            mCurRockInd = mCurRockInd % mRockList.Count;

        //setup display and selected
        UpdateRockDisplay();
    }

    public void SelectLeft() {
        if(interactable && mCurState == State.None && mRockList.Count > 1) {
            mCurTime = 0f;
            mIsRotateLeft = true;
            mCurState = State.Selecting;
        }
    }

    public void SelectRight() {
        if(interactable && mCurState == State.None && mRockList.Count > 1) {
            mCurTime = 0f;
            mIsRotateLeft = false;
            mCurState = State.Selecting;
        }
    }

    void UpdateRockDisplay() {
        if(mRockList.Count == 0) {
            if(rockTitleText) rockTitleText.text = "";
            if(rockCountText) rockCountText.text = "";

            for(int i = 0; i < rockItems.Length; i++) {
                if(rockItems[i])
                    rockItems[i].gameObject.SetActive(false);
            }

            return;
        }

        //setup mid
        int midInd = rockItems.Length / 2;

        var rockItem = mRockList[mCurRockInd];
        rockItems[midInd].Init(rockItem);
        rockItems[midInd].gameObject.SetActive(true);

        if(rockTitleText)
            rockTitleText.text = rockItem.titleString;

        UpdateCurrentRockCountDisplay();
        //

        //setup left side
        for(int i = midInd - 1, selectInd = mCurRockInd - 1; i >= 0; i--, selectInd--) {
            if(selectInd < 0) {
                if(mRockList.Count <= rockDisplayCount) {
                    rockItems[i].gameObject.SetActive(false);
                    continue;
                }

                selectInd = mRockList.Count - 1;
            }

            rockItem = mRockList[selectInd];
            rockItems[i].Init(rockItem);
            rockItems[i].gameObject.SetActive(true);
        }

        //setup right side
        for(int i = midInd + 1, selectInd = mCurRockInd + 1; i < rockItems.Length; i++, selectInd++) {
            if(selectInd >= mRockList.Count) {
                if(mRockList.Count <= rockDisplayCount) {
                    rockItems[i].gameObject.SetActive(false);
                    continue;
                }

                selectInd = 0;
            }

            rockItem = mRockList[selectInd];
            rockItems[i].Init(rockItem);
            rockItems[i].gameObject.SetActive(true);
        }

        rockRotate.rotation = Quaternion.identity;

        mCurState = State.None;
        mCurTime = 0f;
    }

    void UpdateCurrentRockCountDisplay() {
        if(rockCountText)
            rockCountText.text = mRockList[mCurRockInd].count.ToString(rockCountFormat);
    }
    
    protected override void OnDisable() {
        base.OnDisable();

        ClearRocks();
    }

    protected override void OnEnable() {
        base.OnEnable();

        EventSystem es = EventSystem.current;
        if(es)
            SetSelected(es.currentSelectedGameObject == gameObject);

        mIsInteractible = interactable;
        UpdateDisable();

        rockRotate.rotation = Quaternion.identity;

        Refresh(true);
    }

    protected override void Awake() {
        base.Awake();
        
        for(int i = 0; i < rockItems.Length; i++) {
            rockItems[i].gameObject.SetActive(false);
        }

        mRotateTweenFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);
    }

    public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);

        SetSelected(true);
    }

    public override void OnDeselect(BaseEventData eventData) {
        base.OnDeselect(eventData);

        SetSelected(false);
    }

    public override void OnPointerDown(PointerEventData eventData) {
        base.OnPointerDown(eventData);

        mIsDown = true;
    }

    public override void OnPointerUp(PointerEventData eventData) {
        base.OnPointerUp(eventData);

        mIsDown = false;
    }

    void Update() {
        if(mIsInteractible != interactable) {
            mIsInteractible = interactable;
            UpdateDisable();
        }

        switch(mCurState) {
            case State.None:
                if(!interactable || !mIsSelected || mRockList.Count == 0)
                    break;

                if(mIsDown || processInput.IsDown()) {
                    mCurTime = 0f;
                    mCurState = State.Processing;
                }
                else if(mRockList.Count > 1 && selectAxisInput.IsPressed()) {
                    float axisVal = selectAxisInput.GetAxis();
                    if(Mathf.Abs(axisVal) > 0.3f) {
                        mIsRotateLeft = Mathf.Sign(axisVal) < 0f;

                        //check if we can't move left/right
                        if(mRockList.Count <= rockDisplayCount) {
                            if((mIsRotateLeft && mCurRockInd == 0) || mCurRockInd == mRockList.Count - 1)
                                break;
                        }

                        mCurTime = 0f;
                        mCurState = State.Selecting;
                    }
                }
                break;

            case State.Processing:
                if(mIsDown || processInput.IsDown()) {
                    mCurTime += Time.deltaTime;

                    float t = Mathf.Clamp01(mCurTime / rockProcessDelay);
                    
                    int midInd = rockItems.Length / 2;
                    rockItems[midInd].fill = t;

                    if(t >= 1f)
                        mCurState = State.Proceed;
                }
                else {
                    int midInd = rockItems.Length / 2;
                    rockItems[midInd].fill = 0f;
                    mCurState = State.None;
                }
                break;

            case State.Proceed: {
                    int midInd = rockItems.Length / 2;
                    rockItems[midInd].fill = 0f;

                    if(processRockCallback != null)
                        processRockCallback(mRockList[mCurRockInd]);

                    mCurState = State.None;
                }
                break;

            case State.Selecting:
                //do animation
                if(mCurTime < rockRotateDelay) {
                    mCurTime += Time.deltaTime;

                    float t = mRotateTweenFunc(mCurTime, rockRotateDelay, 0f, 0f);

                    float rotZ = Mathf.LerpAngle(0f, mIsRotateLeft ? -rockRotateAmount : rockRotateAmount, t);

                    rockRotate.localEulerAngles = new Vector3(0f, 0f, rotZ);
                }
                else {
                    //change select
                    if(mIsRotateLeft) {
                        if(mCurRockInd > 0)
                            mCurRockInd--;
                    }
                    else {
                        if(mCurRockInd < mRockList.Count - 1)
                            mCurRockInd++;
                    }

                    UpdateRockDisplay();

                    mCurState = State.None;
                }
                break;
        }
    }

    void SetSelected(bool selected) {
        mIsSelected = selected;
        if(selectActiveGO) selectActiveGO.SetActive(selected);
    }

    void UpdateDisable() {
        if(disableGO) disableGO.SetActive(mIsInteractible);
    }
    
    private void ClearRocks() {
        for(int i = 0; i < rockItems.Length; i++) {
            if(rockItems[i])
                rockItems[i].gameObject.SetActive(false);
        }

        mRockList.Clear();
    }
}
