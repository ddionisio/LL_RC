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

    [Header("Move Animate")]
    public SpriteRenderer moveSprite;
    public float moveSpeedNormal = 30f; //speed scale
    public float moveSpeedMinScale = 0.1f;
    public float moveToAirDelay = 0.2f; //delay before switching to air animation when not on ground.

    [Header("Animations")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSpawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDespawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDeath;

    //move animate takes
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeIdle;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeMove;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeAir;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeWallSlide;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSlide;

    [Header("Signal Listen")]
    public M8.Signal signalGameReady;
    public M8.Signal signalInputLock;
    public M8.Signal signalInputUnlock;

    [Header("Signal Invoke")]
    public M8.Signal signalDeath;
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
    private float mGroundLastTime; //last time we are on ground

    private int mTakeIndIdle;
    private int mTakeIndMove;
    private int mTakeIndAir;
    private int mTakeIndWallSlide;
    private int mTakeIndSlide;

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

        mTakeIndIdle = animator.GetTakeIndex(takeIdle);
        mTakeIndMove = animator.GetTakeIndex(takeMove);
        mTakeIndAir = animator.GetTakeIndex(takeAir);
        mTakeIndWallSlide = animator.GetTakeIndex(takeWallSlide);
        mTakeIndSlide = animator.GetTakeIndex(takeSlide);

        animator.ResetTake(takeSpawn);

        if(input)
            input.ActionAddCallback(OnAct);
    }

    void Update() {
        if(stateControl.state == stateNormal) {
            if(bodyControl.isSlide) {
                if(animator.currentPlayingTakeIndex != mTakeIndSlide) {
                    animator.animScale = 1f;
                    animator.Play(mTakeIndSlide);
                }

                //flip sprite based on side
                moveSprite.flipX = (bodyControl.sideFlags & M8.RigidBodyController2D.SideFlags.Right) != M8.RigidBodyController2D.SideFlags.None;
            }
            else {
                switch(input.moveState) {
                    case PlayerInput.MoveState.Normal:
                        //ground
                        if(bodyControl.isGrounded || (input.jumpState == PlayerInput.JumpState.None && Time.time - mGroundLastTime < moveToAirDelay)) { //ensure air delay when not on ground
                            //moving
                            if(input.moveHorz != 0f) {
                                if(animator.currentPlayingTakeIndex != mTakeIndMove)
                                    animator.Play(mTakeIndMove);

                                //determine anim scale based on speed
                                float animScale = Mathf.Abs(bodyControl.localVelocity.x / moveSpeedNormal);
                                if(animScale < moveSpeedMinScale)
                                    animScale = moveSpeedMinScale;

                                animator.animScale = animScale;

                                //determine facing based on movement
                                if(input.moveHorz != 0f)
                                    moveSprite.flipX = input.moveHorz < 0f;
                            }
                            //idle
                            else {
                                if(animator.currentPlayingTakeIndex != mTakeIndIdle) {
                                    animator.animScale = 1f;
                                    animator.Play(mTakeIndIdle);
                                }
                            }

                            if(bodyControl.isGrounded)
                                mGroundLastTime = Time.time;
                        }
                        //air
                        else {
                            if(animator.currentPlayingTakeIndex != mTakeIndAir) {
                                animator.animScale = 1f;
                                animator.Play(mTakeIndAir);
                            }

                            //determine facing based on movement
                            if(input.moveHorz != 0f && input.jumpState == PlayerInput.JumpState.None)
                                moveSprite.flipX = input.moveHorz < 0f;
                        }
                        break;

                    case PlayerInput.MoveState.SideStick:
                        if(animator.currentPlayingTakeIndex != mTakeIndWallSlide) {
                            animator.animScale = 1f;
                            animator.Play(mTakeIndWallSlide);
                        }
                        
                        //flip sprite based on side
                        if(bodyControl.sideFlags != M8.RigidBodyController2D.SideFlags.None)
                            moveSprite.flipX = (bodyControl.sideFlags & M8.RigidBodyController2D.SideFlags.Right) != M8.RigidBodyController2D.SideFlags.None;
                        break;
                }
            }
        }
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
            //reset move anim. states
            moveSprite.flipX = false;
            
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
        if(signalDeath)
            signalDeath.Invoke();

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
