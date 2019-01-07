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

    [Header("Move")]
    public float standMoveFalloffDelay = 0.3f;
    public float jumpForce = 20f;

    [Header("Input")]
    public M8.InputAction moveHorzInput;
    public M8.InputAction actInput;

    public bool actIsEnabled { get; private set; }
    public Vector2 jumpDir { get; private set; }

    private float mMoveHorz;
    private JumpState mJumpState;

    void OnEnable() {
        actIsEnabled = false;
        jumpDir = bodyControl.dirHolder.up;

        mMoveHorz = 0f;

        if(bodyControl) {
            if(mJumpState != JumpState.None)
                bodyControl.lockDragCounter--;
        }

        mJumpState = JumpState.None;
    }

    void Update() {
        //movement
        mMoveHorz = moveHorzInput.GetAxis();

        //determine if we can act
        if(bodyControl.isGrounded) {
            actIsEnabled = true;
            jumpDir = bodyControl.dirHolder.up;
        }
        else {
            //check if we are on contact of a side
            //determine actDir
            actIsEnabled = false;
        }

        if(actIsEnabled) {
            if(mJumpState == JumpState.None) {
                var actState = actInput.GetButtonState();
                if(actState == M8.InputAction.ButtonState.Pressed) {
                    mJumpState = JumpState.JumpStart;
                }
            }
        }
    }

    void FixedUpdate() {
        //update body control
        bodyControl.moveHorizontal = mMoveHorz;

        switch(mJumpState) {
            case JumpState.JumpStart:
                bodyControl.lockDragCounter++;
                bodyControl.body.drag = 0f;
                mJumpState = JumpState.Jump;
                break;
            case JumpState.Jump:
                bodyControl.body.AddForce(jumpDir * jumpForce, ForceMode2D.Impulse);
                bodyControl.lockDragCounter--;
                mJumpState = JumpState.None;
                break;
        }
    }
}
