using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ensure SceneState is available
/// </summary>
public struct Checkpoint {
    public const string sceneVarPositionX = "checkpointPosX";
    public const string sceneVarPositionY = "checkpointPosY";
    public const string sceneVarRotation = "checkpointRot";

    public static bool globalAvailable {
        get {
            return M8.SceneState.instance.global.Contains(sceneVarPositionX);
        }
    }

    public static bool localAvailable {
        get {
            return M8.SceneState.instance.local.Contains(sceneVarPositionX);
        }
    }

    public static Vector2 globalPosition {
        get {
            var sceneState = M8.SceneState.instance.global;
            return new Vector2(sceneState.GetValueFloat(sceneVarPositionX), sceneState.GetValueFloat(sceneVarPositionY));
        }

        set {
            var sceneState = M8.SceneState.instance.global;
            sceneState.SetValueFloat(sceneVarPositionX, value.x, false);
            sceneState.SetValueFloat(sceneVarPositionY, value.y, false);
        }
    }

    public static float globalRotation {
        get {
            var sceneState = M8.SceneState.instance.global;
            return sceneState.GetValueFloat(sceneVarRotation);
        }

        set {
            var sceneState = M8.SceneState.instance.global;
            sceneState.SetValueFloat(sceneVarRotation, value, false);
        }
    }

    public static Vector2 localPosition {
        get {
            var sceneState = M8.SceneState.instance.local;
            return new Vector2(sceneState.GetValueFloat(sceneVarPositionX), sceneState.GetValueFloat(sceneVarPositionY));
        }

        set {
            var sceneState = M8.SceneState.instance.local;
            sceneState.SetValueFloat(sceneVarPositionX, value.x, false);
            sceneState.SetValueFloat(sceneVarPositionY, value.y, false);
        }
    }

    public static float localRotation {
        get {
            var sceneState = M8.SceneState.instance.local;
            return sceneState.GetValueFloat(sceneVarRotation);
        }

        set {
            var sceneState = M8.SceneState.instance.local;
            sceneState.SetValueFloat(sceneVarRotation, value, false);
        }
    }

    public static void RemoveGlobal() {
        var sceneState = M8.SceneState.instance.global;
        sceneState.RemoveValue(sceneVarPositionX, false);
        sceneState.RemoveValue(sceneVarPositionY, false);
        sceneState.RemoveValue(sceneVarRotation, false);
    }
}
