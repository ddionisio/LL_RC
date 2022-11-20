using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Retarded way to change color of sprite shape
/// </summary>
public class SpriteShapeColor : MonoBehaviour {
    public UnityEngine.U2D.SpriteShapeController target;

    public Color color {
        get {
            if(!mMat)
                GetProperty();

            return mColor;
        }

        set {
            if(!mMat)
                GetProperty();

            if(mColor != value) {
                mColor = value;
                mMat.SetColor("_Color", mColor);
            }
        }
    }

    private Material mMat;
    private Color mColor;

    void OnDestroy() {
        if(mMat)
            Destroy(mMat);
    }

    void Awake() {
        if(!target)
            target = GetComponent<UnityEngine.U2D.SpriteShapeController>();
    }

    private void GetProperty() {
        mMat = target.spriteShapeRenderer.material;
        mColor = mMat.GetColor("_Color");
    }
}
