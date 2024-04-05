using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class MetamorphRockHintWidget : MonoBehaviour {
    public RockSelectWidget rockSelect;

    [Header("Display")]
    public RawImage rockResultImage;
    public TMP_Text rockResultLabel;
    public GameObject unknownActiveGO;

    private RockData mCurRock;

	void OnEnable() {
        mCurRock = null;
		Refresh(null);
	}

	void Update() {
		if(rockSelect) {
            var curRock = rockSelect.currentRock as RockData;
            if(mCurRock != curRock) {
                mCurRock = curRock;
                Refresh(curRock);
			}
        }
	}

	private void Refresh(RockData rock) {        
        if(rock) {
			var metamorphRock = rock.metaOutput;

			if(metamorphRock && metamorphRock.isSeen) {
				if(rockResultImage) {
					rockResultImage.texture = metamorphRock.spriteShape.fillTexture;
					rockResultImage.gameObject.SetActive(true);
				}

				if(rockResultLabel) {
					rockResultLabel.text = metamorphRock.titleString;
					rockResultLabel.gameObject.SetActive(true);
				}

				if(unknownActiveGO) unknownActiveGO.SetActive(false);
			}
			else {
				if(rockResultImage) rockResultImage.gameObject.SetActive(false);
				if(rockResultLabel) rockResultLabel.gameObject.SetActive(false);
				if(unknownActiveGO) unknownActiveGO.SetActive(true);
			}
		}
        else {
            if(rockResultImage) rockResultImage.gameObject.SetActive(false);
			if(rockResultLabel) rockResultLabel.gameObject.SetActive(false);
			if(unknownActiveGO) unknownActiveGO.SetActive(true);
		}
    }
}
