﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerController : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string parmForceScale = "forceScale";

    public enum MoveMode {
        None,
        GroundOnly,
        AfterFirstGround
    }

    public M8.StateController stateControl;
    public Rigidbody2D body;
    public M8.ForceController2D gravityControl;

    public M8.State stateMove;

    public float groundAngleLimit = 60f;
    public MoveMode moveMode = MoveMode.GroundOnly;
    public float force;
    public float speedLimit;
    public float firstGroundImpulse;

    private bool mIsActive;
    private bool mIsGrounded;

    private const int contactCapacity = 8;
    private ContactPoint2D[] mContacts = new ContactPoint2D[contactCapacity];

    private float mForceScale;

    private Vector2 mLastWallNormal;

    private bool mIsFirstGround;

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        mForceScale = 1f;

        if(parms != null) {
            if(parms.ContainsKey(parmForceScale))
                mForceScale = parms.GetValue<float>(parmForceScale);
        }
    }

    void M8.IPoolDespawn.OnDespawned() {
        mIsActive = false;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    void OnStateChanged(M8.State state) {
        var active = state == stateMove;
        if(mIsActive != active) {
            mIsActive = active;
            if(mIsActive) {
                if(body)
                    body.simulated = true;
            }
        }
    }

    void Awake() {
        stateControl.stateChangedEvent.AddListener(OnStateChanged);
    }

    void FixedUpdate() {
        if(!mIsActive)
            return;

        var upDir = -gravityControl.orientDir;
        var moveDir = new Vector2(upDir.y, -upDir.x);
        var wallCheckDir = moveDir * Mathf.Sign(mForceScale);

        bool isFirstGround = mIsFirstGround;
        bool isWallContact = false;

        int contactCount = body.GetContacts(mContacts);
        if(contactCount > 0) {
            //ground check
            for(int i = 0; i < contactCount; i++) {
                var contact = mContacts[i];
                var normal = contact.normal;

                if(!mIsGrounded) {
                    if(Vector2.Angle(upDir, normal) < groundAngleLimit) {
                        mIsGrounded = true;
                        isFirstGround = true;
                    }
                }

                if(!isWallContact) {
                    if(Vector2.Angle(wallCheckDir, normal) >= 150f)
                        isWallContact = true;
                }

                if(mIsGrounded && isWallContact) //no other checks needed?
                    break;
            }
        }
        else {
            mIsGrounded = false;
        }

        //move
        if(isWallContact)
            mForceScale *= -1.0f;

        if(isFirstGround && mIsFirstGround != isFirstGround) {
            mIsFirstGround = isFirstGround;

            body.AddForce(moveDir * Mathf.Sign(mForceScale) * firstGroundImpulse, ForceMode2D.Impulse);
        }

        bool canMove = false;

        switch(moveMode) {
            case MoveMode.GroundOnly:
                canMove = mIsGrounded;
                break;
            case MoveMode.AfterFirstGround:
                canMove = mIsFirstGround;
                break;
            default:
                canMove = true;
                break;
        }

        if(canMove) {
            var curVel = body.velocity;
            var curSpeed = curVel.magnitude;
            var forceDir = curSpeed > 0f ? curVel / curSpeed : moveDir;

            //if(Vector2.Angle(moveDir, forceDir) > 90f || curSpeed < speedLimit)
            if(curSpeed < speedLimit)
                body.AddForce(mForceScale * force * moveDir, ForceMode2D.Force);
        }
    }
}
