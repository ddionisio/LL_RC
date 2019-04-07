using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CollectGather : MonoBehaviour {
    [Header("Rock")]
    public InventoryData inventory;
    public SpriteShapeController rockSpriteShape;
    public DG.Tweening.Ease rockMoveEase;
    public M8.Animator.Animate rockAnimator;
    [M8.Animator.TakeSelector(animatorField = "rockAnimator")]
    public string rockTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "rockAnimator")]
    public string rockTakeExit;

    [Header("Data")]
    [M8.TagSelector]
    public string tagPlayer;

    public float rockMoveDelay = 0.5f;

    public Transform destination;

    [Header("Signal Listen")]
    public M8.Signal signalStart;

    [Header("Signal Invoke")]
    public M8.Signal signalFinish;

    private M8.PoolController mPool;

    private DG.Tweening.EaseFunction mRockMoveEaseFunc;
    private Transform mRockTrans;

    private Transform mPlayerTrans;

    void Awake() {
        var playerGO = GameObject.FindGameObjectWithTag(tagPlayer);
        mPlayerTrans = playerGO.transform;

        mRockTrans = rockSpriteShape.transform;

        mRockMoveEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(rockMoveEase);
    }

    void OnDisable() {
        signalStart.callback -= OnSignalStart;
    }

    void OnEnable() {
        rockSpriteShape.gameObject.SetActive(false);

        signalStart.callback += OnSignalStart;
    }

    void OnSignalStart() {
        StartCoroutine(DoGather());
    }

    IEnumerator DoGather() {
        //go through newly seen rocks
        yield return DoRockListGather(inventory.rocksIgneous);
        yield return DoRockListGather(inventory.rocksSedimentary);
        yield return DoRockListGather(inventory.rocksMetamorphic);

        signalFinish.Invoke();
    }

    IEnumerator DoRockListGather(RockData[] rocks) {
        rockSpriteShape.gameObject.SetActive(true);

        Vector2 startPos = mPlayerTrans.position;
        Vector2 endPos = destination.position;


        for(int i = 0; i < rocks.Length; i++) {
            var rock = rocks[i];
            if(!rock.isNewlySeen)
                continue;

            rockSpriteShape.spriteShape = rock.spriteShape;

            mRockTrans.position = startPos;
            mRockTrans.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

            yield return rockAnimator.PlayWait(rockTakeEnter);

            float curTime = 0f;
            while(curTime < rockMoveDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = mRockMoveEaseFunc(curTime, rockMoveDelay, 0f, 0f);

                mRockTrans.position = Vector2.Lerp(startPos, endPos, t);
            }

            yield return rockAnimator.PlayWait(rockTakeExit);
        }
    }
}
