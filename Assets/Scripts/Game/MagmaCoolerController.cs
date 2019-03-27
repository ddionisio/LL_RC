using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MagmaCoolerController : GameModeController<MagmaCoolerController> {
    public enum Mode {
        None,
        Intrusive,
        Extrusive
    }

    [System.Serializable]
    public struct IntrusiveDisplayInfo {
        public GameObject rootGO;
        public SpriteShapeColor spriteShapeColor;

        public void Init(bool active) {
            if(active) {
                rootGO.SetActive(true);

                var clr = spriteShapeColor.color;
                clr.a = 1f;
                spriteShapeColor.color = clr;
            }
            else
                rootGO.SetActive(false);
        }
    }

    [Header("Data")]
    public InventoryData inventory;
    public CriteriaData criteria;
    public RockIgneousData[] intrusiveRocks; //which type of rock to produce based on cooling delay
    public RockIgneousData[] extrusiveRocks;
    public int extrusiveResultCount = 3;
    
    [Header("Process")]
    public SequenceInfo processSequence;

    public Button processIntrusiveButton;
    public int processIntrusiveDisableProgress; //which progress this is disabled

    public Button processExtrusiveButton;
    public int processExtrusiveDisableProgress; //which progress this is disabled

    [Header("Intrusive")]
    public SequenceInfo intrusiveSequence;
    public Button intrusiveReturnButton;
    public Button intrusiveProceedButton;

    [Header("Cooling")]
    public SequenceInfo coolingSequence;
    public Button coolingStopButton;
    public GameObject coolingGlowGO;
    public Slider coolingSlider;
    public float coolingDelay = 1f; //delay per rock
    public float coolingSegmentDelay = 0.5f; //delay before resuming cooling

    public IntrusiveDisplayInfo[] intrusiveRockDisplays;
    public float intrusiveRockDisplayFadeOutDelay = 0.3f;

    public M8.Animator.Animate intrusiveAnimator;
    [M8.Animator.TakeSelector(animatorField = "intrusiveAnimator")]
    public string intrusiveTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "intrusiveAnimator")]
    public string intrusiveTakeExit;
    [M8.Animator.TakeSelector(animatorField = "intrusiveAnimator")]
    public string intrusiveTakeFill;

    [Header("Extrusive")]
    public SequenceInfo extrusiveSequence;
    public Button extrusiveReturnButton;
    public Button extrusiveProceedButton;

    public M8.Animator.Animate extrusiveAnimator;
    [M8.Animator.TakeSelector(animatorField = "extrusiveAnimator")]
    public string extrusiveTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "extrusiveAnimator")]
    public string extrusiveTakeExit;

    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;
    
    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    private bool mIsCoolingStop;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();
    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    private Mode mCurMode = Mode.None;

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        processSequence.Init();
        intrusiveSequence.Init();
        coolingSequence.Init();
        extrusiveSequence.Init();
        rockResultSequence.Init();

        if(intrusiveAnimator) intrusiveAnimator.gameObject.SetActive(false);
        if(extrusiveAnimator) extrusiveAnimator.gameObject.SetActive(false);

        processIntrusiveButton.onClick.AddListener(OnProcessIntrusive);
        processExtrusiveButton.onClick.AddListener(OnProcessExtrusive);

        intrusiveProceedButton.onClick.AddListener(OnIntrusiveProceed);
        intrusiveReturnButton.onClick.AddListener(OnIntrusiveBack);

        coolingStopButton.onClick.AddListener(OnCoolingStop);

        extrusiveProceedButton.onClick.AddListener(OnExtrusiveProceed);
        extrusiveReturnButton.onClick.AddListener(OnExtrusiveBack);

        rockResultContinueButton.onClick.AddListener(OnRockResultContinue);

        exitButton.onClick.AddListener(OnExit);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoProcessSelect() {
        //check if there's magma
        if(inventory.magma.count > 0) {
            int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

            //determine if intrusive/extrusive locked
            processIntrusiveButton.interactable = processIntrusiveDisableProgress != progress;
            processExtrusiveButton.interactable = processExtrusiveDisableProgress != progress;
            //

            yield return processSequence.Enter();

            if(processIntrusiveButton.interactable)
                processIntrusiveButton.Select();
            else if(processExtrusiveButton.interactable)
                processExtrusiveButton.Select();
            else
                exitButton.Select();
        }
        else { //show nothing, just exit
            exitButton.Select();
        }
    }

    //intrusive process    
    IEnumerator DoProcessToIntrusiveProceed() {
        ClearSelection();

        yield return processSequence.Exit();

        yield return intrusiveSequence.Enter();
        
        intrusiveProceedButton.Select();
    }

    IEnumerator DoIntrusiveBack() {
        ClearSelection();

        yield return intrusiveSequence.Exit();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoMode(Mode toMode) {
        if(mCurMode == toMode)
            yield break;

        M8.Animator.Animate exitAnimator = null;
        string exitTake = "";

        M8.Animator.Animate enterAnimator = null;
        string enterTake = "";

        switch(toMode) {
            case Mode.Intrusive:
                enterAnimator = intrusiveAnimator;
                enterTake = intrusiveTakeEnter;
                break;
            case Mode.Extrusive:
                enterAnimator = extrusiveAnimator;
                enterTake = extrusiveTakeEnter;
                break;
        }

        switch(mCurMode) {
            case Mode.Extrusive:
                exitAnimator = extrusiveAnimator;
                exitTake = extrusiveTakeEnter;
                break;

            case Mode.Intrusive:
                exitAnimator = intrusiveAnimator;
                exitTake = intrusiveTakeEnter;
                break;
        }

        if(exitAnimator) {
            if(!string.IsNullOrEmpty(exitTake))
                yield return exitAnimator.PlayWait(exitTake);

            exitAnimator.gameObject.SetActive(false);
        }

        if(enterAnimator) {
            enterAnimator.gameObject.SetActive(true);

            if(!string.IsNullOrEmpty(enterTake))
                yield return enterAnimator.PlayWait(enterTake);
        }

        mCurMode = toMode;
    }

    IEnumerator DoIntrusiveProceed() {
        ClearSelection();

        coolingSlider.normalizedValue = 0f;
                
        yield return intrusiveSequence.Exit();

        //initialize display
        intrusiveRockDisplays[0].Init(true);
        for(int i = 1; i < intrusiveRockDisplays.Length; i++)
            intrusiveRockDisplays[i].Init(false);

        //change mode
        yield return DoMode(Mode.Intrusive);

        //do intrusive fill
        if(intrusiveAnimator && !string.IsNullOrEmpty(intrusiveTakeFill))
            yield return intrusiveAnimator.PlayWait(intrusiveTakeFill);

        //initial state
        coolingGlowGO.SetActive(false);
        coolingStopButton.interactable = false;

        //cooling
        yield return coolingSequence.Enter();

        yield return new WaitForSeconds(1f);
                                
        int intrusiveRockInd = 0;
        int rockDisplayInd = 0;
        float curTime = 0f;
        float curPauseTime = 0f;
        float coolingSegment = 1.0f / intrusiveRocks.Length;

        //next rock
        intrusiveRockDisplays[1].rootGO.SetActive(true);

        //do cooling and fill animation
        mIsCoolingStop = false;
        while(!mIsCoolingStop && intrusiveRockInd < intrusiveRocks.Length) {                
            if(curTime < coolingDelay) {
                curTime += Time.deltaTime;
                float t = Mathf.Clamp01(curTime / coolingDelay);
                coolingSlider.normalizedValue = (intrusiveRockInd + t) * coolingSegment;

                //apply alpha to rock display
                int nextRockInd = rockDisplayInd + 1;
                
                var clr = intrusiveRockDisplays[nextRockInd].spriteShapeColor.color;
                clr.a = t;
                intrusiveRockDisplays[nextRockInd].spriteShapeColor.color = clr;
                //

                //ready to stop
                if(t >= 1f) {
                    intrusiveRockDisplays[rockDisplayInd].rootGO.SetActive(false);
                    rockDisplayInd++;
                    
                    coolingGlowGO.SetActive(true);

                    coolingStopButton.interactable = true;
                    coolingStopButton.Select();
                }
            }
            else if(curPauseTime < coolingSegmentDelay) {
                curPauseTime += Time.deltaTime;
            }
            else {
                curTime = 0f;
                curPauseTime = 0f;
                intrusiveRockInd++;

                if(rockDisplayInd + 1 < intrusiveRockDisplays.Length)
                    intrusiveRockDisplays[rockDisplayInd + 1].rootGO.SetActive(true);

                coolingStopButton.interactable = false;
            }

            yield return null;
        }

        yield return coolingSequence.Exit();
        //

        //fade out last rock
        if(rockDisplayInd < intrusiveRockDisplays.Length) {
            curTime = 0f;
            while(curTime < intrusiveRockDisplayFadeOutDelay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = Mathf.Clamp01(curTime / intrusiveRockDisplayFadeOutDelay);

                var clr = intrusiveRockDisplays[rockDisplayInd].spriteShapeColor.color;
                clr.a = 1f - t;
                intrusiveRockDisplays[rockDisplayInd].spriteShapeColor.color = clr;
            }

            intrusiveRockDisplays[rockDisplayInd].rootGO.SetActive(false);
        }

        //generate rock
        mRockResultList.Clear();

        if(intrusiveRockInd >= intrusiveRocks.Length)
            intrusiveRockInd = intrusiveRocks.Length - 1;

        var rock = intrusiveRocks[intrusiveRockInd];
        rock.count += inventory.igneousOutput;

        var magmaCount = inventory.magma.count - inventory.igneousOutput;
        if(magmaCount < 0)
            magmaCount = 0;
        inventory.magma.count = magmaCount;

        mRockResultList.Add(rock);
        //

        //show rock result
        StartCoroutine(DoRockResult());
    }
    //

    //extrusive process
    IEnumerator DoProcessToExtrusiveProceed() {
        ClearSelection();

        yield return processSequence.Exit();

        yield return extrusiveSequence.Enter();
        
        extrusiveProceedButton.Select();
    }

    IEnumerator DoExtrusiveBack() {
        ClearSelection();

        yield return extrusiveSequence.Exit();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoExtrusiveProceed() {
        ClearSelection();

        yield return extrusiveSequence.Exit();

        //initialize display

        //change mode
        yield return DoMode(Mode.Extrusive);

        //do extrusive fill
        yield return new WaitForSeconds(0.5f);

        //explode
        yield return new WaitForSeconds(0.5f);

        //just shove random rocks into the list
        var magmaCount = inventory.magma.count;

        M8.ArrayUtil.Shuffle(extrusiveRocks);
        mRockResultList.Clear();
        for(int i = 0; i < extrusiveResultCount; i++) {            
            var rock = extrusiveRocks[i];
            rock.count += inventory.igneousOutput;

            magmaCount -= inventory.igneousOutput;
            if(magmaCount < 0)
                magmaCount = 0;

            mRockResultList.Add(rock);
        }

        inventory.magma.count = magmaCount;
        //

        //show rocks
        StartCoroutine(DoRockResult());
    }
    //

    IEnumerator DoRockResult() {
        ClearSelection();

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

        criteria.signalUpdateIgneous.Invoke();

        //wait for rock continue
        rockResultContinueButton.Select();
        mIsRockResultContinue = false;
        while(!mIsRockResultContinue)
            yield return null;

        yield return rockResultSequence.Exit();

        StartCoroutine(DoProcessSelect());
    }

    void OnExit() {
        exitScene.Load();
    }

    void OnProcessIntrusive() {        
        StartCoroutine(DoProcessToIntrusiveProceed());
    }

    void OnIntrusiveBack() {
        StartCoroutine(DoIntrusiveBack());
    }

    void OnProcessExtrusive() {        
        StartCoroutine(DoProcessToExtrusiveProceed());
    }

    void OnExtrusiveBack() {
        StartCoroutine(DoExtrusiveBack());
    }

    void OnIntrusiveProceed() {        
        StartCoroutine(DoIntrusiveProceed());
    }

    void OnExtrusiveProceed() {        
        StartCoroutine(DoExtrusiveProceed());
    }

    void OnCoolingStop() {
        ClearSelection();
        mIsCoolingStop = true;
    }

    void OnRockResultContinue() {
        ClearSelection();
        mIsRockResultContinue = true;
    }

    void ClearSelection() {
        var es = EventSystem.current;
        if(es)
            es.SetSelectedGameObject(null);
    }
}
