using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagmaWidget : MonoBehaviour {
    [Header("Info")]
    public MagmaData magma;
    public string percentTextFormat = "{0}%";

    [Header("UI")]
    public TMPro.TMP_Text percentText;

    private float mCurPercent;

    void OnDisable() {
        magma.countCallback -= OnMagmaCountUpdate;
    }

    void OnEnable() {
        magma.countCallback += OnMagmaCountUpdate;

        ApplyDisplay(false);
    }

    void OnMagmaCountUpdate(InfoData dat) {
        ApplyDisplay(true);
    }

    void ApplyDisplay(bool isUpdate) {
        float percent = (float)magma.count / magma.capacity;

        if(isUpdate) { //animate
            mCurPercent = percent;
        }
        else { //apply directly
            mCurPercent = percent;
        }

        if(percentText) percentText.text = string.Format(percentTextFormat, Mathf.RoundToInt(100f * mCurPercent));
    }
}
