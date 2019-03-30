using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Rock : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string parmDir = "dir";
    public const string parmSpriteShape = "spriteShape";

    [Header("Data")]
    public SpriteShapeController spriteShapeCtrl;
    public M8.RangeFloat dirAngleRange = new M8.RangeFloat { min = -10f, max = 10f };
    public M8.RangeFloat impulseRange = new M8.RangeFloat { min = 10f, max = 15f };

    public bool isAsleep {
        get {
            return mBody.IsSleeping();
        }
    }

    private Rigidbody2D mBody;
    private M8.PoolDataController mPoolData;

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
        Vector2 dir = Vector2.zero;

        if(parms != null) {
            if(parms.ContainsKey(parmSpriteShape))
                spriteShape = parms.GetValue<SpriteShape>(parmSpriteShape);
            if(parms.ContainsKey(parmDir))
                dir = parms.GetValue<Vector2>(parmDir);
        }

        if(spriteShape) {
            spriteShapeCtrl.spriteShape = spriteShape;
        }

        transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

        spriteShapeCtrl.enabled = true;

        mBody.simulated = true;

        dir = M8.MathUtil.RotateAngle(dir, dirAngleRange.random);
        mBody.AddForce(dir * impulseRange.random, ForceMode2D.Impulse);
    }

    void M8.IPoolDespawn.OnDespawned() {
        
    }
}
