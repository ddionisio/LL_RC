using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressActiveGO : MonoBehaviour {
    [System.Serializable]
    public struct Info {
        public int[] progress;
        public GameObject gameObject;

        public bool CheckMatch(int curProgress) {
            for(int i = 0; i < progress.Length; i++) {
                if(progress[i] == curProgress)
                    return true;
            }

            return false;
        }
    }

    public Info[] infos;

    [Header("Signals")]
    public M8.Signal signalRefresh;

    void OnDisable() {
        if(signalRefresh)
            signalRefresh.callback -= OnRefresh;
    }

    void OnEnable() {
        Refresh();

        if(signalRefresh)
            signalRefresh.callback += OnRefresh;
    }

    public void Refresh() {
        int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

        for(int i = 0; i < infos.Length; i++) {
            var inf = infos[i];

            bool active = inf.CheckMatch(progress);

            if(inf.gameObject)
                inf.gameObject.SetActive(active);
        }
    }

    void OnRefresh() {
        Refresh();
    }
}
