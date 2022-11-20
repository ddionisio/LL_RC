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

    public static string startSceneVarPositionX {
        get {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return sceneVarPositionX + activeScene.name;
        }
    }

    public static string startSceneVarPositionY {
        get {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return sceneVarPositionY + activeScene.name;
        }
    }

    public static string startSceneVarRotation {
        get {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return sceneVarRotation + activeScene.name;
        }
    }

    public static bool startAvailable {
        get {
            return M8.SceneState.instance.global.Contains(startSceneVarPositionX);
        }
    }

    public static bool localAvailable {
        get {
            return M8.SceneState.instance.local.Contains(sceneVarPositionX);
        }
    }

    public static Vector2 startPosition {
        get {
            var sceneState = M8.SceneState.instance.global;
            return new Vector2(sceneState.GetValueFloat(startSceneVarPositionX), sceneState.GetValueFloat(startSceneVarPositionY));
        }

        set {
            var sceneState = M8.SceneState.instance.global;
            sceneState.SetValueFloat(startSceneVarPositionX, value.x, false);
            sceneState.SetValueFloat(startSceneVarPositionY, value.y, false);
        }
    }

    public static float startRotation {
        get {
            var sceneState = M8.SceneState.instance.global;
            return sceneState.GetValueFloat(startSceneVarRotation);
        }

        set {
            var sceneState = M8.SceneState.instance.global;
            sceneState.SetValueFloat(startSceneVarRotation, value, false);
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

    public static void SetStart(Transform t) {
        startPosition = t.position;
        startRotation = t.eulerAngles.z;
    }

    public static void SetLocal(Transform t) {
        localPosition = t.position;
        localRotation = t.eulerAngles.z;
    }

    public static void RemoveStart() {
        var sceneState = M8.SceneState.instance.global;
        sceneState.RemoveValue(startSceneVarPositionX, false);
        sceneState.RemoveValue(startSceneVarPositionY, false);
        sceneState.RemoveValue(startSceneVarRotation, false);
    }
}
