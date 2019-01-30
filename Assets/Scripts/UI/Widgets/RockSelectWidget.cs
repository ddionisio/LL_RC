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

    [System.Flags]
    public enum Filter {
        None = 0x0,
        Igneous = 0x1,
        Sedimentary = 0x2,
        Metamorphic = 0x4,
        Minerals = 0x8,
        Organics = 0x10
    }
    
    [Header("Inventory")]
    public InventoryData inventory;
    [M8.EnumMask]
    public Filter inventoryFilter = Filter.Igneous | Filter.Sedimentary | Filter.Metamorphic;

    [Header("Input")]
    public M8.InputAction processInput; //use the accept input action
    public M8.InputAction selectAxisInput; //axis input to select prev/next (-1 = left/down, 1 = right/up)

    [Header("Rock")]
    public TMPro.TMP_Text rockTitleText;
    public TMPro.TMP_Text rockCountText;
    public string rockCountFormat = "00";

    public float rockProcessDelay = 1f; //delay before processing while pointer is down
    public int rockDisplayCount = 3;
    
    public RockSelectItemWidget rockItemTemplate;

    public float rockPlacementRadius = 150f;
    public int rockPlacementCount = 5;

    public Transform rockRotate;
    public float rockRotateAmount;
    public float rockRotateDelay = 0.3f;
        
    public bool isProcess { get; private set; } //pointer remains down after delay

    public int rockCount {
        get {
            return mRockList != null ? mRockList.Count : 0;
        }
    }

    private bool mIsSelected;
    private bool mIsDown;

    public int mCurRockInd;

    public event System.Action<InfoData> processRockCallback;

    private RockSelectItemWidget[] mRockItems;

    private List<InfoData> mRockList = new List<InfoData>();

    private State mCurState = State.None;
    private float mCurTime;
    private bool mIsRotateLeft;
    
    private DG.Tweening.EaseFunction mRotateTweenFunc;

    private bool mIsInit;

    public void RefreshRock(InfoData dat) {
        Init();

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

    public void Refresh(bool resetIndex, List<InfoData> hideFilter) {
        Init();
        ClearRocks();

        //add rocks with count > 0
        if((inventoryFilter & Filter.Igneous) != Filter.None) {
            for(int i = 0; i < inventory.rocksIgneous.Length; i++) {
                var rock = inventory.rocksIgneous[i];
                if(rock.count > 0) {
                    if(hideFilter != null && hideFilter.Contains(rock))
                        continue;

                    mRockList.Add(rock);
                }
            }
        }

        if((inventoryFilter & Filter.Sedimentary) != Filter.None) {
            for(int i = 0; i < inventory.rocksSedimentary.Length; i++) {
                var rock = inventory.rocksSedimentary[i];
                if(rock.count > 0) {
                    if(hideFilter != null && hideFilter.Contains(rock))
                        continue;

                    mRockList.Add(rock);
                }
            }
        }

        if((inventoryFilter & Filter.Metamorphic) != Filter.None) {
            for(int i = 0; i < inventory.rocksMetamorphic.Length; i++) {
                var rock = inventory.rocksMetamorphic[i];
                if(rock.count > 0) {
                    if(hideFilter != null && hideFilter.Contains(rock))
                        continue;

                    mRockList.Add(rock);
                }
            }
        }

        if((inventoryFilter & Filter.Minerals) != Filter.None) {
            for(int i = 0; i < inventory.minerals.Length; i++) {
                var rock = inventory.minerals[i];
                if(rock.count > 0) {
                    if(hideFilter != null && hideFilter.Contains(rock))
                        continue;

                    mRockList.Add(rock);
                }
            }
        }

        if((inventoryFilter & Filter.Organics) != Filter.None) {
            for(int i = 0; i < inventory.organics.Length; i++) {
                var rock = inventory.organics[i];
                if(rock.count > 0) {
                    if(hideFilter != null && hideFilter.Contains(rock))
                        continue;

                    mRockList.Add(rock);
                }
            }
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

            for(int i = 0; i < mRockItems.Length; i++) {
                if(mRockItems[i])
                    mRockItems[i].gameObject.SetActive(false);
            }

            return;
        }

        //setup mid
        int midInd = mRockItems.Length / 2;

        var rockItem = mRockList[mCurRockInd];
        mRockItems[midInd].Init(rockItem, true);
        mRockItems[midInd].gameObject.SetActive(true);

        if(rockTitleText)
            rockTitleText.text = rockItem.titleString;

        UpdateCurrentRockCountDisplay();
        //

        //setup left side
        for(int i = midInd - 1, selectInd = mCurRockInd - 1; i >= 0; i--, selectInd--) {
            if(selectInd < 0) {
                if(mRockList.Count <= rockDisplayCount) {
                    mRockItems[i].gameObject.SetActive(false);
                    continue;
                }

                selectInd = mRockList.Count - 1;
            }

            rockItem = mRockList[selectInd];
            mRockItems[i].Init(rockItem, false);
            mRockItems[i].gameObject.SetActive(true);
        }

        //setup right side
        for(int i = midInd + 1, selectInd = mCurRockInd + 1; i < mRockItems.Length; i++, selectInd++) {
            if(selectInd >= mRockList.Count) {
                if(mRockList.Count <= rockDisplayCount) {
                    mRockItems[i].gameObject.SetActive(false);
                    continue;
                }

                selectInd = 0;
            }

            rockItem = mRockList[selectInd];
            mRockItems[i].Init(rockItem, false);
            mRockItems[i].gameObject.SetActive(true);
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

        if(Application.isPlaying) {
            ClearRocks();
        }
    }

    protected override void OnEnable() {
        base.OnEnable();

        if(Application.isPlaying) {
            EventSystem es = EventSystem.current;
            if(es)
                SetSelected(es.currentSelectedGameObject == gameObject);

            rockRotate.rotation = Quaternion.identity;

            Refresh(true, null);
        }
    }

    protected override void Awake() {
        base.Awake();

        if(Application.isPlaying) {
            Init();
        }
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
        if(!Application.isPlaying)
            return;

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
                            if((mIsRotateLeft && mCurRockInd == 0) || (!mIsRotateLeft && mCurRockInd == mRockList.Count - 1))
                                break;
                        }

                        mCurTime = 0f;
                        mCurState = State.Selecting;
                    }
                }
                break;

            case State.Processing:
                if(!interactable || !mIsSelected || mRockList.Count == 0) {
                    int midInd = mRockItems.Length / 2;
                    mRockItems[midInd].fill = 0f;
                    mCurState = State.None;
                    break;
                }

                if(mIsDown || processInput.IsDown()) {
                    mCurTime += Time.deltaTime;

                    float t = Mathf.Clamp01(mCurTime / rockProcessDelay);
                    
                    int midInd = mRockItems.Length / 2;
                    mRockItems[midInd].fill = t;

                    if(t >= 1f)
                        mCurState = State.Proceed;
                }
                else {
                    int midInd = mRockItems.Length / 2;
                    mRockItems[midInd].fill = 0f;
                    mCurState = State.None;
                }
                break;

            case State.Proceed: {
                    int midInd = mRockItems.Length / 2;
                    mRockItems[midInd].fill = 0f;

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
                        if(mCurRockInd == 0) {
                            if(mRockList.Count > rockDisplayCount)
                                mCurRockInd = mRockList.Count - 1;
                        }
                        else
                            mCurRockInd--;
                    }
                    else {
                        if(mCurRockInd == mRockList.Count - 1) {
                            if(mRockList.Count > rockDisplayCount)
                                mCurRockInd = 0;
                        }
                        else
                            mCurRockInd++;
                    }

                    UpdateRockDisplay();

                    mCurState = State.None;
                }
                break;
        }
    }

    void OnDrawGizmos() {
        if(rockRotate && rockPlacementRadius > 0f && rockPlacementCount > 0) {
            Gizmos.color = Color.yellow;
            
            float rockPlaceRot = -rockRotateAmount * Mathf.Round(rockPlacementCount / 2);
            for(int i = 0; i < rockPlacementCount; i++) {
                Gizmos.DrawLine(rockRotate.position, rockRotate.position + (Vector3)M8.MathUtil.RotateAngle(Vector2.up, rockPlaceRot) * rockPlacementRadius);
                rockPlaceRot += rockRotateAmount;
            }
        }
    }

    void SetSelected(bool selected) {
        mIsSelected = selected;

        if(!mIsSelected)
            mIsDown = false;
    }

    private void Init() {
        if(mIsInit)
            return;

        //generate rock items
        float rockPlaceRot = -rockRotateAmount * Mathf.Round(rockPlacementCount / 2);
        mRockItems = new RockSelectItemWidget[rockPlacementCount];
        for(int i = 0; i < mRockItems.Length; i++) {
            mRockItems[i] = Instantiate(rockItemTemplate, rockRotate);

            var up = M8.MathUtil.RotateAngle(Vector2.up, rockPlaceRot);

            var t = mRockItems[i].transform;
            t.localPosition = up * rockPlacementRadius;
            t.up = up;
            t.localScale = Vector3.one;

            rockPlaceRot += rockRotateAmount;

            mRockItems[i].gameObject.name = i.ToString();
            mRockItems[i].gameObject.SetActive(false);
        }

        rockItemTemplate.gameObject.SetActive(false);
        //

        mRotateTweenFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        mIsInit = true;
    }

    private void ClearRocks() {
        for(int i = 0; i < mRockItems.Length; i++) {
            if(mRockItems[i])
                mRockItems[i].gameObject.SetActive(false);
        }

        mRockList.Clear();
    }
}
