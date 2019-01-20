using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagmaChamberController : GameModeController<MagmaChamberController> {
    [Header("Data")]
    public InventoryData inventory;
    public M8.SceneAssetPath exitScene;

    [Header("UI")]
    public Selectable magmaProcessor;
    public RockSelectWidget rockSelector;
    public Button exitButton;

    public void MagmaProcess() {

    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //initialize minerals

        //initialize rocks

        //initialize navigations
    }

    protected override IEnumerator Start() {
        yield return base.Start();


    }

    void OnExitClicked() {
        exitScene.Load();
    }
}
