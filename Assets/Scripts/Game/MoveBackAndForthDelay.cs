using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackAndForthDelay : MonoBehaviour {
    [Header("Info")]
    public Transform target; //ensure this is set to kinematic

    public float speed = 5f;
    public float startDelay = 1f;
    public float apexDelay = 0f;
    public float repeatDelay = 1f;

    public bool activeOnEnable = true;
    public bool backEnabled = true; //if false, reset to start on repeat

    public DG.Tweening.Ease forwardEase = DG.Tweening.Ease.OutSine;
    public DG.Tweening.Ease backEase = DG.Tweening.Ease.InSine;

    [Header("Points")]
    public Transform beginPoint; //set null for self
    public Transform endPoint;

    [Header("Display")]
    public GameObject activeGO; //when moving, set active (display)
    public ParticleSystem readyFX; //ensure it is not looping

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeAction;
    public bool waitForAnimation = true;

    private Rigidbody2D mTargetBody;

    public void Act() {
        StopAllCoroutines();

        Init();
        StartCoroutine(DoAction());
    }

    void OnDisable() {
        Init();
    }

    void OnEnable() {
        if(activeOnEnable)
            Act();
        else
            Init();
    }

    void Awake() {
        mTargetBody = target.GetComponent<Rigidbody2D>();
    }

    IEnumerator DoAction() {
        yield return new WaitForSeconds(startDelay);

        var forwardTween = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(forwardEase);
        var backTween = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(backEase);

        var waitFixed = new WaitForFixedUpdate();
        var waitRepeat = new WaitForSeconds(repeatDelay);
        var waitApex = apexDelay > 0f ? new WaitForSeconds(apexDelay) : null;

        Vector2 pt1 = beginPoint.position;
        Vector2 pt2 = endPoint.position;
        
        float dist = (pt1 - pt2).magnitude;
        float delay = dist / speed;

        while(true) {
            if(animator && !string.IsNullOrEmpty(takeAction)) {
                animator.Play(takeAction);
                if(waitForAnimation) {
                    while(animator.isPlaying)
                        yield return null;
                }
            }

            if(readyFX) {
                readyFX.Play();
                while(readyFX.isPlaying)
                    yield return null;
            }

            float curTime = 0f;

            if(activeGO) activeGO.SetActive(true);

            //forward
            while(curTime < delay) {
                yield return waitFixed;

                curTime += Time.fixedDeltaTime;

                float t = forwardTween(curTime, delay, 0f, 0f);

                var pt = Vector2.Lerp(pt1, pt2, t);

                if(mTargetBody)
                    mTargetBody.MovePosition(pt);
                else
                    target.position = pt;
            }

            if(waitApex != null)
                yield return waitApex;

            //backward
            if(backEnabled) {
                curTime = 0f;
                while(curTime < delay) {
                    yield return waitFixed;

                    curTime += Time.fixedDeltaTime;

                    float t = backTween(curTime, delay, 0f, 0f);

                    var pt = Vector2.Lerp(pt2, pt1, t);

                    if(mTargetBody)
                        mTargetBody.MovePosition(pt);
                    else
                        target.position = pt;
                }
            }
            else {
                target.transform.position = beginPoint.position;
            }

            if(activeGO) activeGO.SetActive(false);

            yield return waitRepeat;
        }        
    }

    private void Init() {
        if(!beginPoint)
            beginPoint = transform;

        target.transform.position = beginPoint.position;

        if(activeGO)
            activeGO.SetActive(false);

        if(animator && !string.IsNullOrEmpty(takeAction))
            animator.ResetTake(takeAction);
    }

    void OnDrawGizmos() {
        var startPt = beginPoint ? beginPoint.position : transform.position;
        var endPt = endPoint ? endPoint.position : transform.position;

        if(startPt != endPt) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPt, endPt);
        }
    }
}
