using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSetColorOnEnable : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Color color = Color.white;

    void OnEnable() {
        if(sprite)
            sprite.color = color;
    }
}
