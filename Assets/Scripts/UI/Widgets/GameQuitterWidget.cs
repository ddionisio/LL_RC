using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Use for quitters who easily give up.
/// </summary>
public class GameQuitterWidget : MonoBehaviour {
    [M8.TagSelector]
    public string tagPlayer = "Player";
    public M8.State stateQuit;
    public int deathCount = 5; //after this amount of death
    public float showDelay = 300f; //after 5 min.    

    [Header("Display")]
    public GameObject instructGO;
    public Button quitButton;

    [Header("Signals")]
    public M8.Signal signalPlayReady; //wait for this signal

    private bool mIsChecking;
    private bool mIsQuitClicked;

    private PlayerController mPlayer;

    void OnDestroy() {
        signalPlayReady.callback -= OnSignalPlayReady;
    }

    void OnDisable() {
        ResetDisplay();
        mIsChecking = false;
        mIsQuitClicked = false;
    }

    void Awake() {
        ResetDisplay();

        quitButton.onClick.AddListener(OnQuitClick);

        signalPlayReady.callback += OnSignalPlayReady;
    }

    void ResetDisplay() {
        instructGO.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    void OnQuitClick() {
        if(!mIsQuitClicked) {
            mIsQuitClicked = true;

            if(!mPlayer)
                GetPlayer();

            mPlayer.stateControl.state = stateQuit;
        }
    }

    void OnSignalPlayReady() {
        if(!mIsChecking) {
            mIsChecking = true;
            StartCoroutine(DoCheck());
        }
    }

    void GetPlayer() {
        var playerGO = GameObject.FindGameObjectWithTag(tagPlayer);
        mPlayer = playerGO.GetComponent<PlayerController>();
    }

    IEnumerator DoCheck() {
        var lastTime = Time.time;

        if(!mPlayer)
            GetPlayer();

        while(mPlayer.deathCount < deathCount && Time.time - lastTime < showDelay)
            yield return null;

        instructGO.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }
}
