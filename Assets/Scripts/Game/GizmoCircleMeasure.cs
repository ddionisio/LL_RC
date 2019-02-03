using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoCircleMeasure : MonoBehaviour {
    public float[] radii = new float[] { 1f };
    public float[] lineAngles;

    public Color colorRadius = Color.white;
    public Color colorLine = Color.white;

    void OnDrawGizmos() {
        var pos = transform.position;

        float lineLen = 0f;

        if(radii != null) {
            Gizmos.color = colorRadius;

            for(int i = 0; i < radii.Length; i++) {
                if(radii[i] > 0) {
                    if(radii[i] > lineLen)
                        lineLen = radii[i];

                    Gizmos.DrawWireSphere(pos, radii[i]);
                }
            }
        }

        if(lineAngles != null) {
            Gizmos.color = colorLine;

            for(int i = 0; i < lineAngles.Length; i++) {
                Vector3 dir = M8.MathUtil.RotateAngle(Vector2.up, lineAngles[i]);
                Gizmos.DrawLine(pos, pos + lineLen * dir);
            }
        }
    }
}
