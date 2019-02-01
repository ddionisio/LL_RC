using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSpawnDespawnController : MonoBehaviour, M8.IPoolSpawnComplete, M8.IPoolDespawn {
    [Header("Data")]
    public M8.StateController stateControl;
    public Rigidbody2D body;
    public bool releaseAfterDespawn = true;

    [Header("Spawn Info")]
    public M8.State spawnAfterState; //state after spawn finished

    [Header("Despawn Info")]
    public M8.State despawnState;
    public M8.State despawnAfterState; //state after despawn finish, if releaseAfterDespawn is false
    public float despawnStartDelay; //delay before animating
    public bool despawnDisablePhysics = true;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSpawn;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDespawn;

    private Coroutine mRout;

    private M8.PoolDataController mPoolCtrl;
    private bool mIsPoolCtrlChecked = false;

    void Awake() {
        if(!stateControl)
            stateControl = GetComponent<M8.StateController>();

        stateControl.stateChangedEvent.AddListener(OnEntityChangedState);
    }

    void OnEntityChangedState(M8.State state) {
        StopRoutine();

        if(stateControl.state == despawnState) {
            if(despawnDisablePhysics) {
                if(body) body.simulated = false;
            }

            mRout = StartCoroutine(DoDespawn());
        }
    }

    void M8.IPoolSpawnComplete.OnSpawnComplete() {
        if(!mIsPoolCtrlChecked) {
            mPoolCtrl = GetComponent<M8.PoolDataController>();
            mIsPoolCtrlChecked = true;
        }

        if(body) body.simulated = false;

        mRout = StartCoroutine(DoSpawn());
    }

    void M8.IPoolDespawn.OnDespawned() {
        if(body) body.simulated = false;

        StopRoutine();
    }

    IEnumerator DoSpawn() {
        if(animator && !string.IsNullOrEmpty(takeSpawn)) {
            animator.Play(takeSpawn);
            while(animator.isPlaying)
                yield return null;
        }
        else
            yield return null;

        mRout = null;

        stateControl.state = spawnAfterState;
    }

    IEnumerator DoDespawn() {
        if(despawnStartDelay > 0f)
            yield return new WaitForSeconds(despawnStartDelay);

        if(animator && !string.IsNullOrEmpty(takeDespawn)) {
            animator.Play(takeDespawn);
            while(animator.isPlaying)
                yield return null;
        }
        else
            yield return null;

        mRout = null;

        if(releaseAfterDespawn) {
            if(mPoolCtrl)
                mPoolCtrl.Release();
            else
                M8.PoolController.ReleaseAuto(gameObject);
        }
        else
            stateControl.state = despawnAfterState;
    }

    private void StopRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
