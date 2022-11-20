using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

[CreateAssetMenu(fileName = "info", menuName = "Game/Info")]
public class InfoData : ScriptableObject {
    [M8.Localize]
    public string titleRef;
    [M8.Localize]
    public string descRef;

    public Sprite icon;
    public Sprite illustration;
    
    public virtual string modal { get { return "info"; } }

    public string titleString { get { return M8.Localize.Get(titleRef); } }
    public string descString { get { return M8.Localize.Get(descRef); } }

    public M8.UserData userData {
        get {
            //return M8.SceneState.instance.userData;
            return LoLManager.instance.userData;
        }
    }

    public string userVarCount {
        get {
            if(string.IsNullOrEmpty(mUserVarCountStr))
                mUserVarCountStr = name + "_c";
            return mUserVarCountStr;
        }
    }

    public string userVarSeen {
        get {
            if(string.IsNullOrEmpty(mUserVarSeenStr))
                mUserVarSeenStr = name + "_s";
            return mUserVarSeenStr;
        }
    }
        
    /// <summary>
    /// Get inventory count, saved value, set to 0 to remove from UserData
    /// </summary>
    public int count {
        get { return M8.SceneState.isInstantiated ? userData.GetInt(userVarCount) : 0; }
        set {
            if(!M8.SceneState.isInstantiated)
                return;

            if(count != value) {
                if(value > 0)
                    userData.SetInt(userVarCount, value);
                else
                    userData.Remove(userVarCount);

                if(countCallback != null)
                    countCallback(this);
            }
        }
    }

    /// <summary>
    /// Check if seen, saved value
    /// </summary>
    public bool isSeen {
        get { return userData.GetInt(userVarSeen) != 0; }
        set {
            if(value) {
                if(!isSeen)
                    isNewlySeen = true;

                userData.SetInt(userVarSeen, 1);
            }
            else {
                isNewlySeen = false;
                userData.Remove(userVarSeen);
            }
        }
    }

    public bool isNewlySeen {
        get;
        private set;
    }

    public event System.Action<InfoData> countCallback;

    private string mUserVarCountStr;
    private string mUserVarSeenStr;

    public void ClearNewlySeen() {
        isNewlySeen = false;
    }
}