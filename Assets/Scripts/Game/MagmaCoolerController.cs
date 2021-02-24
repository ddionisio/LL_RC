using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using LoLExt;

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
    public bool isIntrusiveGiveAllRocks = true;
    public RockIgneousData[] extrusiveRocks;
    public int extrusiveResultCount = 3;

    [Header("Process")]
    public GameObject magmaEmptyGO;
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
    public ParticleSystem coolingFX;
    public GameObject coolingInstructionGO;

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
    [M8.Animator.TakeSelector(animatorField = "extrusiveAnimator")]
    public string extrusiveTakePlay;
    [M8.Animator.TakeSelector(animatorField = "extrusiveAnimator")]
    public string extrusiveTakeClear;

    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;
    
    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string soundMagmaRise;
    [M8.SoundPlaylist]
    public string soundCoolingEnd;

    [Header("Complete")]
    public GameObject completeProceedGO;

    private bool mIsCoolingStop;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();
    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    private Mode mCurMode = Mode.None;
    private int mIntrusiveRockInd = 0;

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

        if(magmaEmptyGO) magmaEmptyGO.SetActive(false);

        if(coolingInstructionGO) coolingInstructionGO.SetActive(false);

        if(completeProceedGO) completeProceedGO.SetActive(false);

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

        StartCoroutine(DoProcessSelect(false));
    }

    IEnumerator DoProcessSelect(bool selectExit) {
        //check if there's magma
        if(inventory.magma.count > 0) {
            int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

            //determine if intrusive/extrusive locked
            processIntrusiveButton.interactable = processIntrusiveDisableProgress != progress;
            processExtrusiveButton.interactable = processExtrusiveDisableProgress != progress;
            //

            yield return processSequence.Enter();

            if(selectExit)
                exitButton.Select();
            else {
                if(processIntrusiveButton.interactable)
                    processIntrusiveButton.Select();
                else if(processExtrusiveButton.interactable)
                    processExtrusiveButton.Select();
                else
                    exitButton.Select();
            }
        }
        else { //show nothing, just exit
            if(magmaEmptyGO) magmaEmptyGO.SetActive(true);

            //exitButton.Select();
        }
    }

    //intrusive process    
    IEnumerator DoProcessToIntrusiveProceed() {
        ClearSelection();

        yield return processSequence.Exit();

        //if extrusive is disabled, proceed right away
        //just proceed
        StartCoroutine(DoIntrusiveProceed());
        /*if(!processExtrusiveButton.interactable) {
            StartCoroutine(DoIntrusiveProceed());
        }
        else {
            yield return intrusiveSequence.Enter();

            intrusiveProceedButton.Select();
        }*/
    }

    IEnumerator DoIntrusiveBack() {
        ClearSelection();

        yield return intrusiveSequence.Exit();

        StartCoroutine(DoProcessSelect(true));
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
                exitTake = extrusiveTakeExit;
                break;

            case Mode.Intrusive:
                exitAnimator = intrusiveAnimator;
                exitTake = intrusiveTakeExit;
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
        M8.SoundPlaylist.instance.Play(soundMagmaRise, false);

        if(intrusiveAnimator && !string.IsNullOrEmpty(intrusiveTakeFill))
            yield return intrusiveAnimator.PlayWait(intrusiveTakeFill);

        //initial state
        coolingGlowGO.SetActive(false);
        coolingStopButton.interactable = false;

        //check if we have seen the instruction
        /*bool isInstructionSeen = false;
        if(M8.SceneState.instance.global.GetValue("magmaCoolerIntrusiveInstructionSeen") == 1)
            isInstructionSeen = true;
        else
            M8.SceneState.instance.global.SetValue("magmaCoolerIntrusiveInstructionSeen", 1, false);

        if(coolingInstructionGO) coolingInstructionGO.SetActive(!isInstructionSeen);*/

        //cooling
        yield return coolingSequence.Enter();

        yield return new WaitForSeconds(1f);

        mIntrusiveRockInd = 0;

        int intrusiveRockInd = 0;        
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
                int nextRockInd = mIntrusiveRockInd + 1;
                
                var clr = intrusiveRockDisplays[nextRockInd].spriteShapeColor.color;
                clr.a = t;
                intrusiveRockDisplays[nextRockInd].spriteShapeColor.color = clr;
                //

                //ready to stop
                if(t >= 1f) {
                    M8.SoundPlaylist.instance.Play(soundCoolingEnd, false);

                    if(coolingFX) coolingFX.Play();

                    intrusiveRockDisplays[mIntrusiveRockInd].rootGO.SetActive(false);
                    mIntrusiveRockInd++;
                    
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

                if(mIntrusiveRockInd + 1 < intrusiveRockDisplays.Length)
                    intrusiveRockDisplays[mIntrusiveRockInd + 1].rootGO.SetActive(true);

                coolingStopButton.interactable = false;
            }

            yield return null;
        }

        yield return coolingSequence.Exit();
        //
                
        //generate rock
        mRockResultList.Clear();

        if(isIntrusiveGiveAllRocks) { //just give all the rocks, sigh...
            for(int i = 0; i < intrusiveRocks.Length; i++) {
                var rock = intrusiveRocks[i];

                if(GlobalSettings.isUnlimitedResource) {
                    if(rock.count <= 0)
                        rock.count = GlobalSettings.unlimitedResourceRock;

                    mRockResultList.Add(rock);
                }
                else {
                    rock.count += inventory.igneousOutput;

                    var magmaCount = inventory.magma.count - inventory.igneousOutput;
                    if(magmaCount < 0)
                        magmaCount = 0;
                    inventory.magma.count = magmaCount;

                    mRockResultList.Add(rock);

                    if(magmaCount == 0)
                        break;
                }
            }
        }
        else {
            if(intrusiveRockInd >= intrusiveRocks.Length)
                intrusiveRockInd = intrusiveRocks.Length - 1;

            var rock = intrusiveRocks[intrusiveRockInd];
            rock.count += inventory.igneousOutput;

            var magmaCount = inventory.magma.count - inventory.igneousOutput;
            if(magmaCount < 0)
                magmaCount = 0;
            inventory.magma.count = magmaCount;

            mRockResultList.Add(rock);
        }
        //

        //show rock result
        StartCoroutine(DoRockResult());
    }
    //

    //extrusive process
    IEnumerator DoProcessToExtrusiveProceed() {
        ClearSelection();

        yield return processSequence.Exit();

        //if intrusive is disabled, proceed right away
        //just proceed
        StartCoroutine(DoExtrusiveProceed());
        /*if(!processIntrusiveButton.interactable) {
            StartCoroutine(DoExtrusiveProceed());
        }
        else {
            yield return extrusiveSequence.Enter();

            extrusiveProceedButton.Select();
        }*/
    }

    IEnumerator DoExtrusiveBack() {
        ClearSelection();

        yield return extrusiveSequence.Exit();

        StartCoroutine(DoProcessSelect(true));
    }

    IEnumerator DoExtrusiveProceed() {
        ClearSelection();

        yield return extrusiveSequence.Exit();

        //initialize display

        //change mode
        yield return DoMode(Mode.Extrusive);

        //explode
        if(extrusiveAnimator && !string.IsNullOrEmpty(extrusiveTakePlay))
            yield return extrusiveAnimator.PlayWait(extrusiveTakePlay);

        //just shove random rocks into the list
        var magmaCount = inventory.magma.count;

        M8.ArrayUtil.Shuffle(extrusiveRocks);
        mRockResultList.Clear();
        for(int i = 0; i < extrusiveResultCount; i++) {            
            var rock = extrusiveRocks[i];

            if(GlobalSettings.isUnlimitedResource) {
                if(rock.count <= 0)
                    rock.count = GlobalSettings.unlimitedResourceRock;
            }
            else {
                rock.count += inventory.igneousOutput;

                magmaCount -= inventory.igneousOutput;
                if(magmaCount < 0)
                    magmaCount = 0;
            }

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

        //show complete proceed if criteria is met
        if(criteria.IsComplete(inventory)) {
            if(completeProceedGO)
                completeProceedGO.SetActive(true);
        }

        //wait for rock continue

        exitButton.Select();
        //rockResultContinueButton.Select();

        mIsRockResultContinue = false;
        while(!mIsRockResultContinue)
            yield return null;

        yield return rockResultSequence.Exit();

        //clear out mode
        switch(mCurMode) {
            case Mode.Intrusive:
                //fade out last rock
                if(mIntrusiveRockInd < intrusiveRockDisplays.Length) {
                    float curTime = 0f;
                    while(curTime < intrusiveRockDisplayFadeOutDelay) {
                        yield return null;

                        curTime += Time.deltaTime;

                        float t = Mathf.Clamp01(curTime / intrusiveRockDisplayFadeOutDelay);

                        var clr = intrusiveRockDisplays[mIntrusiveRockInd].spriteShapeColor.color;
                        clr.a = 1f - t;
                        intrusiveRockDisplays[mIntrusiveRockInd].spriteShapeColor.color = clr;
                    }

                    intrusiveRockDisplays[mIntrusiveRockInd].rootGO.SetActive(false);
                }
                break;

            case Mode.Extrusive:
                if(extrusiveAnimator && !string.IsNullOrEmpty(extrusiveTakeClear))
                    yield return extrusiveAnimator.PlayWait(extrusiveTakeClear);
                break;
        }

        StartCoroutine(DoProcessSelect(false));
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
