using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectPercentWidget : MonoBehaviour {
    [Header("UI")]
    public Text numberText;
    public string numberTextFormat = "{0}%";
    public float numberUpdateDelay = 1f;

    public GameObject completeGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeCollect;

    [Header("Signals")]
    public M8.Signal signalPlayReady; //show hud
    public M8.Signal signalComplete;
    public SignalInteger signalCollect;

    private int mCurCollectCount = 0;

    private float mCurPercent = 0f;
    private float mPrevPercent;
    private float mToPercent;
    private float mLastChangeTime;

    void OnDestroy() {
        signalPlayReady.callback -= OnSignalPlayReady;
        signalComplete.callback -= OnSignalComplete;
        signalCollect.callback -= OnSignalCollect;
    }

    void Awake() {
        if(completeGO) completeGO.SetActive(false);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.ResetTake(takeEnter);

        if(numberText)
            numberText.text = string.Format(numberTextFormat, 0);

        signalPlayReady.callback += OnSignalPlayReady;
        signalComplete.callback += OnSignalComplete;
        signalCollect.callback += OnSignalCollect;
    }

    void Update() {
        if(mCurPercent != mToPercent) {
            float t = Mathf.Clamp01((Time.time - mLastChangeTime) / numberUpdateDelay);

            mCurPercent = Mathf.Lerp(mPrevPercent, mToPercent, t);

            if(numberText)
                numberText.text = string.Format(numberTextFormat, Mathf.RoundToInt(100f * mCurPercent));
        }
    }

    void OnSignalPlayReady() {
        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.Play(takeEnter);
    }

    void OnSignalComplete() {
        if(completeGO) completeGO.SetActive(true);
    }

    void OnSignalCollect(int amt) {
        mCurCollectCount += amt;

        mToPercent = (float)mCurCollectCount / CollectController.instance.collectMaxCount;
        mPrevPercent = mCurPercent;
        mLastChangeTime = Time.time;

        if(animator && !animator.isPlaying)
            animator.Play(takeCollect);
    }
}
