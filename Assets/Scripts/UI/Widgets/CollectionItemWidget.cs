using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionItemWidget : MonoBehaviour {
    public Transform rockShapesRoot;
    public GameObject lockedGO;
    public Text titleText;
    public Selectable selectable;

    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeNewlySeen;

    public Color rockShapeColor {
        get { return mRockShapeImage ? mRockShapeImage.color : Color.clear; }
        set {
            if(mRockShapeImage)
                mRockShapeImage.color = value;
        }
    }

    public RockData rockData { get; private set; }

    private Shapes2D.Shape[] mRockShapes;

    private Shapes2D.Shape mRockShape;
    private Image mRockShapeImage;

    private M8.GenericParams mModalParms = new M8.GenericParams();

    public IEnumerator PlayNewlySeen() {
        if(animator && !string.IsNullOrEmpty(takeNewlySeen))
            yield return animator.PlayWait(takeNewlySeen);
    }
    
    public void Init(RockData aRockData) {
        rockData = aRockData;

        if(!mRockShape) {
            if(mRockShapes == null)
                mRockShapes = rockShapesRoot.GetComponentsInChildren<Shapes2D.Shape>();

            if(mRockShapes.Length > 0) {
                int rockShapeInd = Random.Range(0, mRockShapes.Length);

                for(int i = 0; i < mRockShapes.Length; i++)
                    mRockShapes[i].gameObject.SetActive(rockShapeInd == i);

                mRockShape = mRockShapes[rockShapeInd];
                mRockShapeImage = mRockShape.GetComponent<Image>();
            }
        }
    }

    public void RefreshDisplay() {
        ResetDisplay();

        if(rockData.isSeen) {
            if(rockShapesRoot) rockShapesRoot.gameObject.SetActive(true);
            if(selectable) selectable.gameObject.SetActive(true);

            if(titleText) {
                titleText.text = rockData.titleString;
                titleText.gameObject.SetActive(true);
            }

            if(mRockShape)
                mRockShape.settings.fillTexture = rockData.spriteShape.fillTexture;

            //reset take, wait for it to be played
            if(rockData.isNewlySeen) {
                if(animator && !string.IsNullOrEmpty(takeNewlySeen))
                    animator.ResetTake(takeNewlySeen);
            }
            else {
                rockShapeColor = Color.white;
            }
        }
        else {
            if(lockedGO) lockedGO.SetActive(true);
        }
    }

    public void OpenInfo() {
        if(rockData) {
            //open modal
            mModalParms[ModalInfo.parmInfoData] = rockData;
            M8.ModalManager.main.Open(rockData.modal, mModalParms);
        }
    }
    
    private void ResetDisplay() {
        if(rockShapesRoot) rockShapesRoot.gameObject.SetActive(false);
        if(lockedGO) lockedGO.SetActive(false);
        if(selectable) selectable.gameObject.SetActive(false);
        if(titleText) titleText.gameObject.SetActive(false);
    }
}
