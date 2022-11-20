using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagmaWidget : MonoBehaviour {
    [Header("Info")]
    public MagmaData magma;
    public string percentTextFormat = "{0}%";
    public float percentUpdateDelay = 1f;

    [Header("UI")]
    public Text percentText;
    public Image percentSlider;
    public bool percentSliderInvert = true;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUpdate;

    private float mCurPercent;
    private float mPrevPercent;
    private float mToPercent;
    private float mLastChangeTime;

    private bool mIsStarted;

    void OnDisable() {
        magma.countCallback -= OnMagmaCountUpdate;
    }

    void OnEnable() {
        magma.countCallback += OnMagmaCountUpdate;

        if(mIsStarted)
            ApplyDisplay(false);
    }

    void Start() {
        ApplyDisplay(false);
        mIsStarted = true;
    }

    void Update() {
        if(mCurPercent != mToPercent) {
            float t = Mathf.Clamp01((Time.time - mLastChangeTime) / percentUpdateDelay);

            mCurPercent = Mathf.Lerp(mPrevPercent, mToPercent, t);

            ApplyText();
            ApplySlider();
        }
    }

    void OnMagmaCountUpdate(InfoData dat) {
        ApplyDisplay(true);
    }

    void ApplyDisplay(bool isUpdate) {
        float percent = (float)magma.count / magma.capacity;

        if(isUpdate && percentUpdateDelay > 0f) { //animate
            mPrevPercent = mCurPercent;
            mToPercent = percent;
            mLastChangeTime = Time.time;

            if(animator && !string.IsNullOrEmpty(takeUpdate))
                animator.Play(takeUpdate);
        }
        else { //apply directly
            mCurPercent = mPrevPercent = mToPercent = percent;

            ApplyText();
            ApplySlider();
        }
    }

    void ApplyText() {
        if(percentText) percentText.text = string.Format(percentTextFormat, Mathf.RoundToInt(100f * mCurPercent));
    }

    void ApplySlider() {
        if(percentSlider) {
            var t = Mathf.Clamp01(mCurPercent);
            percentSlider.fillAmount = percentSliderInvert ? 1f - t : t;
        }
    }
}
