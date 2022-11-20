using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Rock : MonoBehaviour, M8.IPoolSpawn {
    public const string parmDir = "dir";
    public const string parmSpriteShape = "spriteShape";

    [Header("Data")]
    public SpriteShapeController spriteShapeCtrl;
    public M8.RangeFloat dirAngleRange = new M8.RangeFloat { min = -10f, max = 10f };
    public M8.RangeFloat impulseRange = new M8.RangeFloat { min = 10f, max = 15f };
    public float aliveDelay = 1f; //how long to stay alive after hitting ground
    public float fadeDelay = 1f; //how long to fade out till release

    private Color mDefaultColor;
    private ContactPoint2D[] mContacts = new ContactPoint2D[8];

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
        mDefaultColor = spriteShapeCtrl.spriteShapeRenderer.color;
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

        spriteShapeCtrl.spriteShapeRenderer.color = mDefaultColor;

        StartCoroutine(DoAlive());
    }

    IEnumerator DoAlive() {
        bool isGrounded = false;
        while(!isGrounded) {
            yield return new WaitForFixedUpdate();

            int contactCount = mBody.GetContacts(mContacts);
            for(int i = 0; i < contactCount; i++) {
                var contact = mContacts[i];
                if(Vector2.Angle(contact.normal, Vector2.up) < 45f) {
                    isGrounded = true;
                    break;
                }
            }
        }

        yield return new WaitForSeconds(aliveDelay);

        //fade out
        var clr = mDefaultColor;

        var curTime = 0f;
        while(curTime < fadeDelay) {
            yield return null;

            curTime += Time.deltaTime;

            var t = Mathf.Clamp01(curTime / fadeDelay);
            clr.a = 1f - t;
            spriteShapeCtrl.spriteShapeRenderer.color = clr;
        }

        Release();
    }
}
