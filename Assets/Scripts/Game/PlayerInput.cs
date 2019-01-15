using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public enum JumpState {
        None,
        JumpStart,
        Jump
    }

    public enum MoveState {
        None,
        Normal,
        SideStick
    }

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

    [Header("Jump")]
    public bool jumpEnabled = true;
    public float jumpStartForce = 20f;
    public float jumpForce = 5f;
    public float jumpDelay = 0.3f;
    public float jumpSideAngle = 45f; //angle rotate based on side normal
    public float jumpSideStartForce = 30f;
    public float jumpLastGroundDelay = 0.2f; //allow jumping if we left ground after a delay

    [Header("Action")]
    [M8.TagSelector]
    public string tagActionFilter;

    [Header("Input")]
    public M8.InputAction moveHorzInput;
    public M8.InputAction actInput;

    public bool canJump { get; private set; }

    private MoveState mMoveState;
    private float mMoveHorz;
    private float mMoveSideCurTime;

    private JumpState mJumpState;
    private float mJumpCurTime;

    private float mLastGroundTime;

    private const int actInvokeCapacity = 4;
    private M8.CacheList<IActInvoke> mActInvokes = new M8.CacheList<IActInvoke>(actInvokeCapacity);

    void OnTriggerEnter2D(Collider2D collision) {
        if(!string.IsNullOrEmpty(tagActionFilter) && !collision.CompareTag(tagActionFilter))
            return;

        if(collision is IActInvoke) {
            var actInvoke = (IActInvoke)collision;

            int actInvokeInd = -1;
            for(int i = 0; i < mActInvokes.Count; i++) {
                if(mActInvokes[i] == actInvoke) {
                    actInvokeInd = i;
                    break;
                }
            }

            if(actInvokeInd != -1)
                mActInvokes.Add(actInvoke);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(!string.IsNullOrEmpty(tagActionFilter) && !collision.CompareTag(tagActionFilter))
            return;

        if(collision is IActInvoke) {
            var actInvoke = (IActInvoke)collision;
            mActInvokes.Remove(actInvoke);
        }
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

        mActInvokes.Clear();
    }

    void Update() {
        //movement
        switch(mMoveState) {
            case MoveState.Normal:
                mMoveHorz = moveHorzInput.GetAxis();

                //check if we are sticking to side
                if(!bodyControl.isGrounded && (bodyControl.collisionFlags & CollisionFlags.Sides) != CollisionFlags.None) {
                    if(bodyControl.sideFlags == M8.RigidBodyController2D.SideFlags.Left && mMoveHorz < 0f || bodyControl.sideFlags == M8.RigidBodyController2D.SideFlags.Right && mMoveHorz > 0f) {
                        mMoveState = MoveState.SideStick;
                    }
                }
                break;
            case MoveState.SideStick:
                if(bodyControl.isGrounded || (bodyControl.collisionFlags & CollisionFlags.Sides) == CollisionFlags.None) { //revert
                    mMoveState = MoveState.Normal;
                    mMoveHorz = moveHorzInput.GetAxis();
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
            if(mActInvokes.Count > 0) {
                for(int i = 0; i < mActInvokes.Count; i++)
                    mActInvokes[i].ActInvoke();
            }
            else if(canJump && mJumpState == JumpState.None)
                mJumpState = JumpState.JumpStart;
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

                    mJumpState = JumpState.None;
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
        }

        //set friction based on rigid body state
        var lastFriction = bodyControl.body.sharedMaterial.friction;
        float newFriction;

        if(mMoveState == MoveState.SideStick && bodyControl.localVelocity.y < 0f) {
            newFriction = frictionSide;
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
