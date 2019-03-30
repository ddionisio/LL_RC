using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SequenceInfo {
    public GameObject go;
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    public void Init() {
        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.ResetTake(takeEnter);

        if(go) go.SetActive(false);
    }

    public IEnumerator Enter() {
        if(go) go.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);
    }

    public IEnumerator Exit() {
        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        if(go) go.SetActive(false);
    }
}