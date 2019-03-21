using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public enum JumpState {
        None,
        JumpStart,
        Jump,
        JumpWait
    }

    public enum MoveState {
        None,
        Normal,
        SideStick
    }

    //return true if process succeeded, will not allow jump
    public delegate bool ActionCallback();

    public M8.RigidBodyController2D bodyControl;
    public M8.ForceController2D gravityControl;

    [Header("Friction")]
    public float frictionAir = 0f;
    public float frictionGround = 1f;
    public float frictionGroundMove = 0.4f;
    public float frictionSide = 0.2f; //when pressing against the side
    
    [Header("Move")]
    public float moveGroundAngleThresholdMin = 0f;
    public float moveGroundAngleThresholdMax = 70f;
    public float moveGroundMin = 20f;
    public float moveGroundMax = 60f;
    public float moveSideDelay = 0.15f; //stickiness to the side while against it on air
    public float moveSideMinDownSpeed = 10f; //don't allow downward to go faster than this while sticking on wall

    [Header("Jump")]
    public bool jumpEnabled = true;
    public float jumpStartForce = 20f;
    public float jumpForce = 5f;
    public float jumpDelay = 0.3f;
    public float jumpSideAngle = 45f; //angle rotate based on side normal
    public float jumpSideStartForce = 30f;
    public float jumpLastGroundDelay = 0.2f; //allow jumping if we left ground after a delay

    [Header("Input")]
    public M8.InputAction moveHorzInput;
    public M8.InputAction actInput;

    public bool canJump { get; private set; }

    public MoveState moveState { get { return mMoveState; } }
    public JumpState jumpState { get { return mJumpState; } }

    public float moveHorz { get { return mMoveHorz; } }
        
    private MoveState mMoveState;
    private float mMoveHorz;
    private float mMoveSideCurTime;

    private JumpState mJumpState;
    private float mJumpCurTime;

    private float mLastGroundTime;

    private M8.CacheList<ActionCallback> mActionCallbacks = new M8.CacheList<ActionCallback>(4);

    public void ActionAddCallback(ActionCallback cb) {
        int actInd = -1;
        for(int i = 0; i < mActionCallbacks.Count; i++) {
            if(mActionCallbacks[i] == cb) {
                actInd = i;
                break;
            }
        }
        if(actInd == -1)
            mActionCallbacks.Add(cb);
    }

    public void ActionRemoveCallback(ActionCallback cb) {
        mActionCallbacks.Remove(cb);
    }

    void OnEnable() {
        canJump = false;

        mMoveState = MoveState.Normal;
        mMoveHorz = 0f;

        mJumpState = JumpState.None;
        mJumpCurTime = 0f;

        mLastGroundTime = 0f;
    }

    void OnDisable() {
        if(bodyControl) {
            bodyControl.moveForce = moveGroundMin;

            if(bodyControl.body)
                bodyControl.body.sharedMaterial.friction = frictionGroundMove;
        }
    }

    void Update() {
        //movement
        switch(mMoveState) {
            case MoveState.Normal:
                mMoveHorz = moveHorzInput.GetAxis();

                //check if we are sticking to side
                if(!bodyControl.isGrounded && (bodyControl.collisionFlags & CollisionFlags.Sides) != CollisionFlags.None && mJumpState == JumpState.None) {
                    if(bodyControl.sideFlags == M8.RigidBodyController2D.SideFlags.Left && mMoveHorz < 0f || bodyControl.sideFlags == M8.RigidBodyController2D.SideFlags.Right && mMoveHorz > 0f) {
                        mMoveState = MoveState.SideStick;
                    }
                }
                break;
            case MoveState.SideStick:
                if(bodyControl.isGrounded) { //revert right away if grounded
                    mMoveState = MoveState.Normal;
                    mMoveHorz = moveHorzInput.GetAxis();
                }
                else if((bodyControl.collisionFlags & CollisionFlags.Sides) == CollisionFlags.None) {
                    //delay a bit (avoids stutter with uneven wall)
                    if(mMoveSideCurTime < moveSideDelay)
                        mMoveSideCurTime += Time.deltaTime;
                    else {
                        mMoveState = MoveState.Normal;
                        mMoveHorz = moveHorzInput.GetAxis();
                    }
                }
                else {
                    var inpHorz = moveHorzInput.GetAxis();
                    if(bodyControl.sideFlags == M8.RigidBodyController2D.SideFlags.Left && inpHorz < 0f || bodyControl.sideFlags == M8.RigidBodyController2D.SideFlags.Right && inpHorz > 0f) {
                        mMoveSideCurTime = 0f; //reset delay
                    }
                    else if(mMoveSideCurTime < moveSideDelay)
                        mMoveSideCurTime += Time.deltaTime;
                    else {
                        mMoveState = MoveState.Normal;
                        mMoveHorz = moveHorzInput.GetAxis();
                    }
                }
                break;
        }
                
        //determine if we can jump
        if(bodyControl.isGrounded) {
            canJump = jumpEnabled;
            mLastGroundTime = Time.time;
        }
        else if(jumpEnabled) {
            //check if we are on contact of a side
            //determine actDir
            if(Time.time - mLastGroundTime <= jumpLastGroundDelay)
                canJump = true;
            else
                canJump = mMoveState == MoveState.SideStick;// (bodyControl.collisionFlags & CollisionFlags.CollidedSides) != CollisionFlags.None;
        }

        //act or jump
        var actState = actInput.GetButtonState();
        if(actState == M8.InputAction.ButtonState.Pressed) {
            bool isAct = false;
            for(int i = 0; i < mActionCallbacks.Count; i++) {
                if(mActionCallbacks[i]())
                    isAct = true;
            }

            if(!isAct && canJump && mJumpState == JumpState.None) {
                mMoveState = MoveState.Normal;
                mJumpState = JumpState.JumpStart;
            }
        }
    }

    void FixedUpdate() {        
        if(!bodyControl.body.simulated || bodyControl.body.isKinematic)
            return;

        //update body control
        if(!bodyControl.isSlide) {
            bodyControl.moveHorizontal = mMoveHorz;
        }
        else
            bodyControl.moveHorizontal = 0f;

        switch(mJumpState) {
            case JumpState.JumpStart:
                //determine if we are jumping from side
                //jumpDir = bodyControl.dirHolder.up;

                //reset y-vel. for body (hack to make jumps consistent)
                var lvel = bodyControl.localVelocity;
                lvel.y = 0f;
                bodyControl.localVelocity = lvel;

                mJumpCurTime = 0f;

                if(bodyControl.collisionFlags == CollisionFlags.Sides) {
                    var jumpDir = bodyControl.normalSide;

                    if((bodyControl.sideFlags & M8.RigidBodyController2D.SideFlags.Left) != M8.RigidBodyController2D.SideFlags.None)
                        jumpDir = M8.MathUtil.RotateAngle(jumpDir, -jumpSideAngle);
                    else
                        jumpDir = M8.MathUtil.RotateAngle(jumpDir, jumpSideAngle);

                    bodyControl.body.AddForce(jumpDir * jumpSideStartForce, ForceMode2D.Impulse);

                    mJumpState = JumpState.JumpWait; //wait a bit
                }
                else {
                    bodyControl.body.AddForce(bodyControl.dirHolder.up * jumpStartForce, ForceMode2D.Impulse);

                    mJumpState = JumpState.Jump;
                }
                break;

            case JumpState.Jump:
                if(mJumpCurTime < jumpDelay && actInput.IsDown() && (bodyControl.collisionFlags & CollisionFlags.Above) == CollisionFlags.None) {
                    bodyControl.body.AddForce(bodyControl.dirHolder.up * jumpForce, ForceMode2D.Force);
                    mJumpCurTime += Time.fixedDeltaTime;
                }
                else
                    mJumpState = JumpState.None;
                break;

            case JumpState.JumpWait: //just delay, no force
                if(mJumpCurTime < jumpDelay && (bodyControl.collisionFlags & CollisionFlags.Above) == CollisionFlags.None)
                    mJumpCurTime += Time.fixedDeltaTime;
                else
                    mJumpState = JumpState.None;
                break;
        }

        //set friction based on rigid body state
        var lastFriction = bodyControl.body.sharedMaterial.friction;
        float newFriction;

        var bodyLocalVel = bodyControl.localVelocity;

        if(mMoveState == MoveState.SideStick && bodyLocalVel.y < 0f) {
            newFriction = frictionSide;

            //cap down speed
            var spd = -bodyLocalVel.y;
            if(spd > moveSideMinDownSpeed) {
                bodyLocalVel.y = -moveSideMinDownSpeed;
                bodyControl.localVelocity = bodyLocalVel;
            }
        }
        else if(bodyControl.isGrounded && !bodyControl.isSlide) {
            if(bodyControl.moveHorizontal != 0f)
                newFriction = frictionGroundMove;
            else
                newFriction = frictionGround;

            //modify ground move force based on slope
            var up = bodyControl.dirHolder.up;

            float angle = Vector2.Angle(up, bodyControl.normalGround);

            float moveForce;

            if(angle >= moveGroundAngleThresholdMin && angle <= moveGroundAngleThresholdMax) {
                float threshold = Mathf.Abs(angle - moveGroundAngleThresholdMin);
                float thresholdLen = Mathf.Abs(moveGroundAngleThresholdMax - moveGroundAngleThresholdMin);
                moveForce = Mathf.Lerp(moveGroundMin, moveGroundMax, threshold / thresholdLen);
            }
            else if(angle > moveGroundAngleThresholdMax)
                moveForce = moveGroundMax;
            else
                moveForce = moveGroundMin;

            bodyControl.moveForce = moveForce;
        }
        else {
            newFriction = frictionAir;
        }

        if(newFriction != lastFriction) {
            bodyControl.body.sharedMaterial.friction = newFriction;
            bodyControl.bodyCollision.enabled = false;
            bodyControl.bodyCollision.enabled = true;
        }
    }
}
