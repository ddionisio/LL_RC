using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum State {
        None,
        Goto,
        Follow
    }

    public Transform startFollow;

    public float gotoDelay = 0.4f;

    public float followDelay = 0.1f;
    public float followRotateDelay = 0.15f;

    public State state { get; private set; }

    public Transform follow {
        get { return mCurFollow; }
        set {
            if(mCurFollow != value) {
                mCurFollow = value;

                //set state
                if(mCurFollow)
                    state = State.Goto;
                else
                    state = State.None;

                ApplyCurState();
            }
        }
    }

    private Transform mCurFollow;
    private float mCurTime;

    private DG.Tweening.EaseFunction mGotoTweenFunc;
    private Vector2 mGotoStartPos;

    private Vector2 mFollowVel;
    private float mFollowRotVel;

    /// <summary>
    /// Allow camera to move back to follow (useful when teleporting)
    /// </summary>
    public void GotoCurrentFollow() {
        if(mCurFollow) {
            state = State.Goto;
            ApplyCurState();
        }
    }

    void OnDisable() {
        state = State.None;
        mCurFollow = null;
        mFollowVel = Vector2.zero;
        mFollowRotVel = 0f;
    }

    void Awake() {
        mGotoTweenFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InOutSine);
    }

    void Start() {
        follow = startFollow;
    }

    void Update() {
        switch(state) {
            case State.Goto:
                Vector2 curPos = transform.position;
                float curRot = transform.eulerAngles.z;

                Vector2 followPos;
                float followRot;

                if(mCurFollow) {
                    followPos = mCurFollow.position;
                    followRot = mCurFollow.eulerAngles.z;
                }
                else {
                    followPos = curPos;
                    followRot = curRot;
                }

                if(mCurTime < gotoDelay && curPos != followPos && curRot != followRot) {
                    mCurTime += Time.deltaTime;

                    float t = mGotoTweenFunc(mCurTime, gotoDelay, 0f, 0f);

                    transform.position = Vector2.Lerp(mGotoStartPos, followPos, t);

                    UpdateRotate();
                }
                else if(mCurFollow)
                    state = State.Follow;
                else
                    state = State.None;
                break;

            case State.Follow:
                if(mCurFollow) {
                    Vector2 curPt = transform.position;
                    Vector2 followPt = mCurFollow.position;
                    Vector2 toPos = Vector2.SmoothDamp(curPt, followPt, ref mFollowVel, followDelay);

                    transform.position = toPos;

                    UpdateRotate();
                }
                else
                    state = State.None;
                break;
        }
    }

    void UpdateRotate() {
        if(!mCurFollow)
            return;

        var targetRotZ = mCurFollow.eulerAngles.z;
        var curRotZ = transform.eulerAngles.z;

        var rotZ = Mathf.SmoothDampAngle(curRotZ, targetRotZ, ref mFollowRotVel, followRotateDelay);

        transform.eulerAngles = new Vector3(0f, 0f, rotZ);
    }

    private void ApplyCurState() {
        switch(state) {
            case State.Follow:
                mFollowVel = Vector2.zero;
                break;
            case State.Goto:
                mGotoStartPos = transform.position;
                mCurTime = 0f;
                break;
        }
    }
}
