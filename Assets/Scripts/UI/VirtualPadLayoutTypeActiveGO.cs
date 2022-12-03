using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualPadLayoutTypeActiveGO : MonoBehaviour {
    public M8.UserData settingsData;
    public VirtualPadLayoutType[] validTypes;
    public GameObject activeGO;

    void OnEnable() {
        int typeIndex = 0;

        if(settingsData)
            typeIndex = settingsData.GetInt(GlobalSettings.inputUserKey);

        var type = (VirtualPadLayoutType)typeIndex;

        bool isValid = false;
        foreach(var validType in validTypes) {
            if(validType == type) {
                isValid = true;
                break;
            }
        }

        if(activeGO)
            activeGO.SetActive(isValid);
    }
}
