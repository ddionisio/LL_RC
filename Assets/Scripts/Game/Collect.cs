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
    public Collider2D coll;
    public M8.GOActivator activator;
    
    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeCollect;

    [Header("Sound")]
    [M8.SoundPlaylist]
    public string soundCollect;
    
    [Header("Signals")]
    public SignalInteger signalCollect;

    public bool isCollected { get; private set; }
    
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
            if((flags & M8.RigidBodyController2D.CollisionFlags.Below) != M8.RigidBodyController2D.CollisionFlags.None) {
                Collected();
            }
        }
    }

    void Awake() {
        if(!coll)
            coll = GetComponent<Collider2D>();
    }

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            var boxCollider = coll as BoxCollider2D;
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

        if(coll) coll.enabled = false;
        if(activator) { activator.Release(); activator.enabled = false; }

        if(!string.IsNullOrEmpty(soundCollect))
            M8.SoundPlaylist.instance.Play(soundCollect, false);

        signalCollect.Invoke(amount);

        //update display
        if(animator && !string.IsNullOrEmpty(takeCollect))
            StartCoroutine(DoCollectedAnimate());
        else {
            gameObject.SetActive(false);
        }
    }

    IEnumerator DoCollectedAnimate() {
        yield return animator.PlayWait(takeCollect);
        
        gameObject.SetActive(false);
    }
}
