using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Rock : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string parmSpriteShape = "spriteShape";

    [Header("Data")]
    public SpriteShapeController spriteShapeCtrl;
    public Transform[] spawnPoints;

    [Header("Jump")]
    public M8.RangeFloat jumpAngle = new M8.RangeFloat { min = -15f, max = 15f };
    public M8.RangeFloat jumpImpulse = new M8.RangeFloat { min = 10f, max = 15f };

    [Header("Despawn")]
    public ParticleSystem disappearFX;
    public float disappearDelay;

    [Header("Signals")]
    public M8.Signal signalJump;

    public bool isAsleep {
        get {
            return mBody.IsSleeping();
        }
    }

    private Rigidbody2D mBody;
    private M8.PoolDataController mPoolData;

    public void Despawn() {
        StartCoroutine(DoDespawn());
    }

    public void Release() {
        if(mPoolData)
            mPoolData.Release();
    }

    void Awake() {
        mBody = GetComponent<Rigidbody2D>();
    }
    
    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        if(!mPoolData)
            mPoolData = GetComponent<M8.PoolDataController>();

        SpriteShape spriteShape = null;

        if(parms != null) {
            if(parms.ContainsKey(parmSpriteShape))
                spriteShape = parms.GetValue<SpriteShape>(parmSpriteShape);
        }

        if(spriteShape) {
            spriteShapeCtrl.spriteShape = spriteShape;
        }

        transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

        spriteShapeCtrl.enabled = true;

        mBody.simulated = true;

        signalJump.callback += OnSignalJump;
    }

    void M8.IPoolDespawn.OnDespawned() {
        signalJump.callback -= OnSignalJump;
    }

    IEnumerator DoDespawn() {
        spriteShapeCtrl.enabled = false;
        mBody.simulated = false;

        if(disappearFX) {
            disappearFX.Play();
            yield return new WaitForSeconds(disappearDelay);
        }

        Release();
    }

    void OnSignalJump() {
        var up = M8.MathUtil.RotateAngle(Vector2.up, jumpAngle.random);
        mBody.AddForce(up * jumpImpulse.random, ForceMode2D.Impulse);
    }
}
