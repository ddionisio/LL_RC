using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Collect : MonoBehaviour {
    [Header("Box Edit")]
    public Vector2 size;
    public Vector2 sizeSpriteBaseOfs;
    public Vector2 sizeColliderExtraOfs;

    public SpriteRenderer[] spriteBases;
    public SpriteRenderer spriteFrame;

    public BoxCollider2D colliderExtra;

    [Header("Info")]
    public int amount = 1;

    [Header("Data")]
    [M8.TagSelector]
    public string playerTag = "Player";
    
    [Header("Display")]
    public GameObject collectGO;
    public GameObject collectedGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeCollect;
    
    [Header("Signals")]
    public SignalInteger signalCollect;

    public bool isCollected { get; private set; }

    private Collider2D mCollider;

    void OnEnable() {
        if(Application.isPlaying) {
            if(collectGO) collectGO.SetActive(!isCollected);
            if(collectedGO) collectedGO.SetActive(isCollected);
        }
    }

    void Awake() {
        if(Application.isPlaying) {
            if(collectGO) collectGO.SetActive(true);
            if(collectedGO) collectedGO.SetActive(false);

            mCollider = GetComponent<Collider2D>();
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(isCollected)
            return;

        if(!string.IsNullOrEmpty(playerTag) && !collision.CompareTag(playerTag))
            return;

        Collected();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if(isCollected)
            return;

        var go = collision.gameObject;

        if(!string.IsNullOrEmpty(playerTag) && !go.CompareTag(playerTag))
            return;

        //only collect if we are hit from below by rigidbody
        var bodyCtrl = collision.gameObject.GetComponent<M8.RigidBodyController2D>();
        if(bodyCtrl) {
            var flags = bodyCtrl.GetCollisionFlag(collision);
            if((flags & CollisionFlags.Below) != CollisionFlags.None) {
                Collected();
            }
        }
    }

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            if(!mCollider)
                mCollider = GetComponent<Collider2D>();

            var boxCollider = mCollider as BoxCollider2D;
            if(boxCollider)
                boxCollider.size = size;

            if(spriteBases != null) {
                for(int i = 0; i < spriteBases.Length; i++) {
                    if(spriteBases[i])
                        spriteBases[i].size = size + sizeSpriteBaseOfs;
                }
            }

            if(spriteFrame)
                spriteFrame.size = size;

            if(colliderExtra)
                colliderExtra.size = size + sizeColliderExtraOfs;
        }
    }
#endif

    void Collected() {
        isCollected = true;

        if(mCollider)
            mCollider.enabled = false;

        signalCollect.Invoke(amount);

        //update display
        if(animator && !string.IsNullOrEmpty(takeCollect))
            StartCoroutine(DoCollectedAnimate());
        else {
            if(collectGO) collectGO.SetActive(false);
            if(collectedGO) collectedGO.SetActive(true);
        }
    }

    IEnumerator DoCollectedAnimate() {
        yield return animator.PlayWait(takeCollect);

        if(collectGO) collectGO.SetActive(false);
        if(collectedGO) collectedGO.SetActive(true);
    }
}
