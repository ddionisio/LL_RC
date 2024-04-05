using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class SedimentaryRockHintWidget : MonoBehaviour {
    [Header("Data")]
    public Texture[] grainSizeTextures; //corresponds to GrainSize

	[Header("Display Source")]
    public GameObject sourceLockActiveGO;
    public RawImage sourceImage;
    public TMP_Text sourceLabel;

    [Header("Display Result")]
    public GameObject resultLockActiveGO;
    public RawImage resultImage;
	public TMP_Text resultLabel;

	[Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector]
    public int takeUnlock = -1;

    public bool active { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

    public RockSedimentaryData data { get; private set; }

	public void PlayUnlock() {
        if(takeUnlock != -1)
            animator.Play(takeUnlock);
    }

    public void SetLocked(bool isLocked) {
        if(isLocked) {
            if(sourceLockActiveGO) sourceLockActiveGO.SetActive(true);
            if(sourceImage) sourceImage.gameObject.SetActive(false);
			if(sourceLabel) sourceLabel.gameObject.SetActive(false);

			if(resultLockActiveGO) resultLockActiveGO.SetActive(true);
			if(resultImage) resultImage.gameObject.SetActive(false);
			if(resultLabel) resultLabel.gameObject.SetActive(false);
		}
        else {
			if(sourceLockActiveGO) sourceLockActiveGO.SetActive(false);
			if(sourceImage) sourceImage.gameObject.SetActive(true);
			if(sourceLabel) sourceLabel.gameObject.SetActive(true);

			if(resultLockActiveGO) resultLockActiveGO.SetActive(false);
			if(resultImage) resultImage.gameObject.SetActive(true);
			if(resultLabel) resultLabel.gameObject.SetActive(true);
		}
    }

    public void Setup(RockSedimentaryData rockResult) {
        data = rockResult;

		//setup source
		if(rockResult.isOrganicOrChemicallyFormed) {
            if(rockResult.input != null && rockResult.input.Length > 0 && rockResult.input[0] is OrganicData) {
                var src = (OrganicData)rockResult.input[0];

				if(sourceImage) sourceImage.texture = src.spriteShape.fillTexture;
				if(sourceLabel) sourceLabel.text = src.titleString;
			}
            else {
                if(sourceImage) sourceImage.texture = null;
				if(sourceLabel) sourceLabel.text = "";
			}
        }
        else {
            int grainSizeInd = (int)rockResult.grainSize;
            if(grainSizeInd >= 0 && grainSizeInd < grainSizeTextures.Length) {
				if(sourceImage) sourceImage.texture = grainSizeTextures[grainSizeInd];
				if(sourceLabel) sourceLabel.text = M8.Localize.Get(rockResult.grainSizeTextRef);
			}
            else {
				if(sourceImage) sourceImage.texture = null;
				if(sourceLabel) sourceLabel.text = "";
			}
        }

        //setup result
        if(resultImage) resultImage.texture = rockResult.spriteShape.fillTexture;
        if(resultLabel) resultLabel.text = rockResult.titleString;
	}
}
