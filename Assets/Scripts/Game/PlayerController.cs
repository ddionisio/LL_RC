using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public M8.StateController stateControl;
    public M8.RigidBodyController2D bodyControl;
    public PlayerInput input;

    [Header("Action")]
    [M8.TagSelector]
    public string tagActionFilter = "Action";

    [Header("Camera")]
    [M8.TagSelector]
    public string cameraTagFollow;

    [Header("States")]
    public M8.State stateSpawn;
    public M8.State stateDespawn;
    public M8.State stateNormal;
    public M8.State stateDeath;

    [Header("Animations")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSpawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDespawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDeath;

    [Header("Signal Listen")]
    public M8.Signal signalGameReady;
    public M8.Signal signalInputLock;
    public M8.Signal signalInputUnlock;

    [Header("Signal Invoke")]
    public M8.Signal signalDespawn;

    public bool inputEnabled {
        get { return mInputEnabled && stateControl.state == stateNormal; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                input.enabled = inputEnabled;
            }
        }
    }

    private bool mInputEnabled = true;

    private CameraFollow mCameraFollow;

    private const int actInvokeCapacity = 4;
    private M8.CacheList<PlayerAction> mActInvokes = new M8.CacheList<PlayerAction>(actInvokeCapacity);

    private Coroutine mCurRout;

    void OnTriggerEnter2D(Collider2D collision) {
        if(!string.IsNullOrEmpty(tagActionFilter) && !collision.CompareTag(tagActionFilter))
            return;

        var action = collision.GetComponent<PlayerAction>();
        if(action) {
            if(!mActInvokes.Exists(action))
                mActInvokes.Add(action);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(!string.IsNullOrEmpty(tagActionFilter) && !collision.CompareTag(tagActionFilter))
            return;

        var action = collision.GetComponent<PlayerAction>();
        if(action)
            mActInvokes.Remove(action);
    }

    void OnDisable() {
        mActInvokes.Clear();

        if(mCurRout != null) {
            StopCoroutine(mCurRout);
            mCurRout = null;
        }
    }

    void OnDestroy() {
        if(input)
            input.ActionRemoveCallback(OnAct);

        signalGameReady.callback -= OnGameReady;
        signalInputLock.callback -= OnInputLock;
        signalInputUnlock.callback -= OnInputUnlock;
    }

    void Awake() {
        if(Checkpoint.startAvailable) {
            transform.position = Checkpoint.startPosition;
            transform.eulerAngles = new Vector3(0f, 0f, Checkpoint.startRotation);

            Checkpoint.RemoveStart();
        }

        var camFollowGO = GameObject.FindGameObjectWithTag(cameraTagFollow);
        mCameraFollow = camFollowGO.GetComponent<CameraFollow>();

        var cameraFollowTrans = mCameraFollow.transform;
        cameraFollowTrans.position = transform.position;
        cameraFollowTrans.rotation = transform.rotation;

        stateControl.stateChangedEvent.AddListener(OnStateChanged);

        signalGameReady.callback += OnGameReady;
        signalInputLock.callback += OnInputLock;
        signalInputUnlock.callback += OnInputUnlock;

        animator.ResetTake(takeSpawn);

        if(input)
            input.ActionAddCallback(OnAct);
    }

    bool OnAct() {
        if(mActInvokes.Count > 0) {
            for(int i = 0; i < mActInvokes.Count; i++)
                mActInvokes[i].ActionInvoke(this);

            return true;
        }

        return false;
    }

    void OnStateChanged(M8.State state) {
        if(mCurRout != null) {
            StopCoroutine(mCurRout);
            mCurRout = null;
        }

        bool physicsEnable = false;

        if(state == stateSpawn) {
            //apply checkpoint
            if(Checkpoint.localAvailable) {
                transform.position = Checkpoint.localPosition;
                transform.eulerAngles = new Vector3(0f, 0f, Checkpoint.localRotation);
            }
            else { //apply current to checkpoint local
                Checkpoint.localPosition = transform.position;
                Checkpoint.localRotation = transform.eulerAngles.z;
            }

            mCurRout = StartCoroutine(DoSpawn());
        }
        else if(state == stateDespawn) {
            mCurRout = StartCoroutine(DoDespawn());
        }
        else if(state == stateDeath) {
            mCurRout = StartCoroutine(DoDeath());
        }
        else if(state == stateNormal) {
            physicsEnable = true;
        }

        if(physicsEnable) {
            bodyControl.body.simulated = true;
            bodyControl.enabled = true;
        }
        else {
            bodyControl.body.simulated = false;
            bodyControl.body.velocity = Vector2.zero;
            bodyControl.body.angularVelocity = 0f;
            bodyControl.enabled = false;
        }

        input.enabled = inputEnabled;
    }

    void OnInputLock() {
        inputEnabled = false;
    }

    void OnInputUnlock() {
        inputEnabled = true;
    }

    void OnGameReady() {
        stateControl.state = stateSpawn;
    }

    IEnumerator DoSpawn() {
        if(mCameraFollow.follow != transform)
            mCameraFollow.follow = transform;
        else
            mCameraFollow.GotoCurrentFollow();

        while(mCameraFollow.state == CameraFollow.State.Goto)
            yield return null;

        yield return animator.PlayWait(takeSpawn);

        stateControl.state = stateNormal;
    }

    IEnumerator DoDeath() {
        if(mCameraFollow.follow == transform)
            mCameraFollow.follow = null;

        yield return animator.PlayWait(takeDeath);
                
        stateControl.state = stateSpawn;
    }

    IEnumerator DoDespawn() {
        if(mCameraFollow.follow == transform)
            mCameraFollow.follow = null;

        yield return animator.PlayWait(takeDespawn);

        signalDespawn.Invoke();
    }
}
