using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SedimentaryRockHintGroupWidget : MonoBehaviour {
    [Header("Template")]
    public SedimentaryRockHintWidget template;
    public int templateCapacity = 4;
    public Transform templateRoot;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector]
    public int takeEnter = -1;
	[M8.Animator.TakeSelector]
	public int takeExit = -1;

    public bool active { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

    private bool mIsInit;

    private M8.CacheList<SedimentaryRockHintWidget> mActives;
	private M8.CacheList<SedimentaryRockHintWidget> mCache;

    public IEnumerator PlayEnterWait() {
        if(takeEnter != -1)
            yield return animator.PlayWait(takeEnter);
    }

	public IEnumerator PlayExitWait() {
		if(takeExit != -1)
			yield return animator.PlayWait(takeExit);
	}

	public void Unlock(RockSedimentaryData rock) {
        for(int i = 0; i < mActives.Count; i++) {
            var itm = mActives[i];
            if(itm.data == rock) {
                itm.SetLocked(false);
                itm.PlayUnlock();
                break;
            }
        }
    }

	public void Setup(RockSedimentaryData[] rocks) {
        if(!mIsInit)
            Init();
        else
            Clear();

        var count = Mathf.Min(templateCapacity, rocks.Length);
        for(int i = 0; i < count; i++) {
            var rock = rocks[i];

            var itm = mCache.RemoveLast();
            itm.transform.SetAsLastSibling();

            itm.Setup(rock);
            itm.SetLocked(!rock.isSeen);

            itm.active = true;

            mActives.Add(itm);
        }
    }

    private void Clear() {
        for(int i = 0; i < mActives.Count; i++) {
            var itm = mActives[i];
            itm.active = false;
            mCache.Add(itm);
        }

        mActives.Clear();
    }

    private void Init() {
        mActives = new M8.CacheList<SedimentaryRockHintWidget>(templateCapacity);
        mCache = new M8.CacheList<SedimentaryRockHintWidget>(templateCapacity);

        for(int i = 0; i < templateCapacity; i++) {
            var newItm = Instantiate(template, templateRoot);
            newItm.active = false;
            mCache.Add(newItm);
        }

        template.active = false;

        mIsInit = true;
	}
}
