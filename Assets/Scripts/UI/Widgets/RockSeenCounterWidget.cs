using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class RockSeenCounterWidget : MonoBehaviour {
    [Header("Data")]
    public InventoryData inventory;

    [Header("UI")]
    [M8.Localize]
    public string textFormatRef;
    public TMP_Text label;

    void OnEnable() {
        int rockSeen = 0;
        int rockCount = 0;

        for(int i = 0; i < inventory.rocksIgneous.Length; i++) {
            var rock = inventory.rocksIgneous[i];
            if(rock.isSeen)
                rockSeen++;

            rockCount++;
        }

        for(int i = 0; i < inventory.rocksSedimentary.Length; i++) {
            var rock = inventory.rocksSedimentary[i];
            if(rock.isSeen)
                rockSeen++;

            rockCount++;
        }

        for(int i = 0; i < inventory.rocksMetamorphic.Length; i++) {
            var rock = inventory.rocksMetamorphic[i];
            if(rock.isSeen)
                rockSeen++;

            rockCount++;
        }

        var textFormat = M8.Localize.Get(textFormatRef);
        label.text = string.Format(textFormat, rockSeen, rockCount);
    }
}
