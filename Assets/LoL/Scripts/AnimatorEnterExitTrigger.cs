using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEnterExitTrigger : MonoBehaviour {
    public GameObject activeGO;
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    public float changeDelay = 2f;

    public bool isPlaying { get { return animator ? animator.isPlaying : false; } }

    private float mLastTriggerTime;

    void OnTriggerEnter2D(Collider2D collision) {
        if(isPlaying || Time.time - mLastTriggerTime < changeDelay)
            return;

        animator.Play(takeEnter);

        mLastTriggerTime = Time.time;
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(isPlaying || Time.time - mLastTriggerTime < changeDelay)
            return;

        animator.Play(takeExit);

        mLastTriggerTime = Time.time;
    }

    void OnEnable() {
        if(activeGO) activeGO.SetActive(false);
        mLastTriggerTime = 0f;
    }

    void OnDisable() {
        if(activeGO) activeGO.SetActive(false);
    }
}
