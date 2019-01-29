﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RockColorInfo {
    public Color color;
    [M8.Localize]
    public string text;
}

public class RockData : InfoData {
    [Header("Rock Info")]
    public RockColorInfo[] colors;
    public Sprite mapSymbol;
    [M8.Localize]
    public string textureTextRef;
    [M8.Localize]
    public string[] compositionTextRefs;

    public InfoData[] input; //specific to process to determine the output
    public RockMetamorphicData metaOutput; //specific metamorphic output
}
