using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MetamorphicController : GameModeController<MagmaCoolerController> {
    [Header("Data")]
    public InventoryData inventory;
    public CriteriaData criteria;

    [Header("Rock Select")]
    public SequenceInfo rockSelectSequence;
    public RockSelectWidget rockSelect;
    public GameObject noRocksGO;

    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;

    [Header("Rock")]
    public SpriteShapeController rockSpriteShape;
    public M8.Animator.Animate rockAnimator;
    [M8.Animator.TakeSelector(animatorField = "rockAnimator")]
    public string rockTakeEnter;

    [Header("Metamorph")]
    public M8.Animator.Animate metaMorphAnimator;
    [M8.Animator.TakeSelector(animatorField = "metaMorphAnimator")]
    public string metaMorphTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "metaMorphAnimator")]
    public string metaMorphTakeExit;

    [Header("Rock Result")]
    public SpriteShapeController rockResultSpriteShape;
    public M8.Animator.Animate rockResultAnimator;
    [M8.Animator.TakeSelector(animatorField = "rockResultAnimator")]
    public string rockResultTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "rockResultAnimator")]
    public string rockResultTakeExit;

    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string soundMorph;

    private RockData mRockSelect;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();

    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(noRocksGO) noRocksGO.SetActive(false);

        if(rockSelect)
            rockSelect.processRockCallback += OnRockSelect;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();
        
        rockAnimator.gameObject.SetActive(false);

        metaMorphAnimator.gameObject.SetActive(false);
        
        rockResultAnimator.gameObject.SetActive(false);

        rockSelectSequence.Init();
        rockResultSequence.Init();

        rockSelect.processRockCallback += OnRockSelect;

        rockResultContinueButton.onClick.AddListener(OnRockResultContinueClick);
        exitButton.onClick.AddListener(OnExitClick);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        StartCoroutine(DoRockSelect());
    }

    IEnumerator DoRockSelect() {
        var rockCount = inventory.rocksCount;

        if(rockCount > 0) {
            rockSelect.Refresh(true, null);

            yield return rockSelectSequence.Enter();

            rockSelect.Select();

            mRockSelect = null;
            while(!mRockSelect)
                yield return null;

            yield return rockSelectSequence.Exit();

            //fail-safe if no metaOutput
            if(!mRockSelect.metaOutput) {
                Debug.LogWarning("Rock does not have a meta output: "+mRockSelect.name);
                StartCoroutine(DoRockSelect());
                yield break;
            }

            //generate rock
            if(mRockSelect.count > 0) {
                mRockSelect.count--;
                criteria.InvokeUpdate(mRockSelect);
            }

            mRockSelect.metaOutput.count++;
            
            mRockResultList.Clear();
            mRockResultList.Add(mRockSelect.metaOutput);
            //

            //animation

            //rock enter
            rockSpriteShape.spriteShape = mRockSelect.spriteShape;
            
            rockAnimator.gameObject.SetActive(true);
                        
            yield return rockAnimator.PlayWait(rockTakeEnter);

            //meta morph enter
            metaMorphAnimator.gameObject.SetActive(true);

            yield return metaMorphAnimator.PlayWait(metaMorphTakeEnter);

            //rock result play
            M8.SoundPlaylist.instance.Play(soundMorph, false);

            rockResultSpriteShape.spriteShape = mRockSelect.metaOutput.spriteShape;
            
            rockResultAnimator.gameObject.SetActive(true);

            yield return rockResultAnimator.PlayWait(rockResultTakeEnter);
            //
            
            rockAnimator.gameObject.SetActive(false);

            //meta morph leave
            yield return metaMorphAnimator.PlayWait(metaMorphTakeExit);

            metaMorphAnimator.gameObject.SetActive(false);

            criteria.InvokeUpdate(mRockSelect.metaOutput);

            StartCoroutine(DoRockResult());
        }
        else {
            //notify that rocks are needed.
            if(noRocksGO) noRocksGO.SetActive(true);

            exitButton.Select();
        }
    }

    IEnumerator DoRockResult() {
        //fill rock list widget
        rockResultWidget.Init(mRockResultList, null);

        yield return rockResultSequence.Enter();

        //check which rocks have not been seen, display info for each
        for(int i = 0; i < mRockResultList.Count; i++) {
            var rock = mRockResultList[i];
            if(!rock.isSeen) {
                mRockModalParms[ModalInfo.parmInfoData] = rock;

                M8.ModalManager.main.Open(rock.modal, mRockModalParms);

                while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(rock.modal))
                    yield return null;
            }
        }

        //wait for rock continue
        rockResultContinueButton.Select();
        mIsRockResultContinue = false;
        while(!mIsRockResultContinue)
            yield return null;

        yield return rockResultSequence.Exit();

        //rock result leave
        yield return rockResultAnimator.PlayWait(rockResultTakeExit);
        
        rockResultAnimator.gameObject.SetActive(false);

        StartCoroutine(DoRockSelect());
    }

    void OnRockSelect(InfoData dat) {
        mRockSelect = (RockData)dat;
    }

    void OnRockResultContinueClick() {
        ClearSelection();
        mIsRockResultContinue = true;
    }

    void OnExitClick() {
        exitScene.Load();
    }

    void ClearSelection() {
        var es = EventSystem.current;
        if(es)
            es.SetSelectedGameObject(null);
    }
}
