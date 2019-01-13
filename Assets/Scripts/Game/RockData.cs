using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "rock", menuName = "Game/Rock")]
public class RockData : ScriptableObject {
    [M8.Localize]
    public string titleRef;
    [M8.Localize]
    public string descRef;

    public Sprite icon;
    public Sprite illustration;

    public string titleString { get { return M8.Localize.Get(titleRef); } }
    public string descString { get { return M8.Localize.Get(descRef); } }
}
