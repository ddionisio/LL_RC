using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public enum JumpState {
        None,
        JumpStart,
        Jump
    }

    public M8.RigidBodyController2D bodyControl;
    public M8.ForceController2D gravityControl;

    [Header("Friction")]
    public float frictionAir = 0f;
    public float frictionGround = 1f;
    public float frictionGroundMove = 0.4f;

    [Header("Move")]
    public float moveGroundAngleThresholdMin = 0f;
    public float moveGroundAngleThresholdMax = 70f;
    public float moveGroundMin = 20f;
    public float moveGroundMax = 60f;

    [Header("Jump")]
    public float jumpStartForce = 20f;
    public float jumpForce = 5f;
    public float jumpDelay = 0.3f;
    public float jumpSideAngle = 45f; //angle rotate based on side normal
    public float jumpSideStartForce = 30f;

    [Header("Input")]
    public M8.InputAction moveHorzInput;
    public M8.InputAction actInput;

    public bool actIsEnabled { get; private set; }

    private float mMoveHorz;
    private JumpState mJumpState;
    private float mJumpCurTime;

    void OnEnable() {
        actIsEnabled = false;

        mMoveHorz = 0f;

        mJumpState = JumpState.None;
        mJumpCurTime = 0f;
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
        mMoveHorz = moveHorzInput.GetAxis();

        //determine if we can act
        if(bodyControl.isGrounded) {
            actIsEnabled = true;
        }
        else {
            //check if we are on contact of a side
            //determine actDir
            actIsEnabled = (bodyControl.collisionFlags & CollisionFlags.CollidedSides) != CollisionFlags.None;
        }

        if(actIsEnabled) {
            if(mJumpState == JumpState.None) {
                var actState = actInput.GetButtonState();
                if(actState == M8.InputAction.ButtonState.Pressed) {
                //if(actInput.IsDown()) { 
                    mJumpState = JumpState.JumpStart;
                }
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

        if(bodyControl.isGrounded && !bodyControl.isSlide) {
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
