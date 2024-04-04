using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class CollectionItemWidget : MonoBehaviour {
    public Transform rockShapesRoot;
    public GameObject lockedGO;
    public bool lockedIsDisplayed;
    public TMP_Text titleText;
    public Selectable selectable;
    public Material rockUIMaterial;

    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeNewlySeen;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeSeen;

    public Color rockShapeColor {
        get { return mRockShapeImage ? mRockShapeImage.color : Color.clear; }
        set {
            if(mRockShapeImage)
                mRockShapeImage.color = value;
        }
    }

    public RockData rockData { get; private set; }

    public bool ignoreNewlySeen { get { return mIgnoreNewlySeen; } set { mIgnoreNewlySeen = value; } }

    private Image[] mRockShapeImages;

    private Image mRockShapeImage;

    private bool mIgnoreNewlySeen;

    private M8.GenericParams mModalParms = new M8.GenericParams();

    private Material mMaterial;

    public IEnumerator PlayNewlySeen() {
        if(animator && !string.IsNullOrEmpty(takeNewlySeen))
            yield return animator.PlayWait(takeNewlySeen);
    }
    
    public void Init(RockData aRockData, bool ignoreNewlySeen) {
        rockData = aRockData;
        mIgnoreNewlySeen = ignoreNewlySeen;

		if(!mMaterial)
			mMaterial = new Material(rockUIMaterial);

		mMaterial.SetTexture(Rock.materialRockTextureOverlay, rockData.spriteShape.fillTexture);

		if(!mRockShapeImage) {
            if(mRockShapeImages == null) {
                mRockShapeImages = new Image[rockShapesRoot.childCount];
                for(int i = 0; i < rockShapesRoot.childCount; i++)
                    mRockShapeImages[i] = rockShapesRoot.GetChild(i).GetComponent<Image>();
            }

            if(mRockShapeImages.Length > 0) {
                int rockShapeInd = Random.Range(0, mRockShapeImages.Length);

                for(int i = 0; i < mRockShapeImages.Length; i++)
					mRockShapeImages[i].gameObject.SetActive(rockShapeInd == i);

                mRockShapeImage = mRockShapeImages[rockShapeInd];
				mRockShapeImage.material = mMaterial;
			}
        }
    }

    public void RefreshDisplay() {
        ResetDisplay();

        bool isSeen;

        if(mIgnoreNewlySeen && rockData.isNewlySeen)
            isSeen = false;
        else
            isSeen = rockData.isSeen;

        if(isSeen) {
            if(rockShapesRoot) rockShapesRoot.gameObject.SetActive(true);
            if(selectable) selectable.gameObject.SetActive(true);

            if(titleText) {
                titleText.text = rockData.titleString;
                titleText.gameObject.SetActive(true);
            }

			//reset take, wait for it to be played
			if(rockData.isNewlySeen) {
                if(animator && !string.IsNullOrEmpty(takeNewlySeen))
                    animator.ResetTake(takeNewlySeen);
            }
            else {
                rockShapeColor = Color.white;

                if(animator && !string.IsNullOrEmpty(takeSeen))
                    animator.ResetTake(takeSeen);
            }
        }
        else {
            if(lockedGO) lockedGO.SetActive(lockedIsDisplayed);
        }
    }

    public void OpenInfo() {
        if(rockData) {
            //open modal
            mModalParms[ModalInfo.parmInfoData] = rockData;
            M8.ModalManager.main.Open(rockData.modal, mModalParms);
        }
    }

	void OnDestroy() {
		if(mMaterial) {
            Destroy(mMaterial);
            mMaterial = null;
		}
	}

	private void ResetDisplay() {
        if(rockShapesRoot) rockShapesRoot.gameObject.SetActive(false);
        if(lockedGO) lockedGO.SetActive(false);
        if(selectable) selectable.gameObject.SetActive(false);
        if(titleText) titleText.gameObject.SetActive(false);
    }
}
