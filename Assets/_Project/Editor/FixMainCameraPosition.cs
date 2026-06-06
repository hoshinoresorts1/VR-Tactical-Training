using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class FixMainCameraPosition
{
    private const float DefaultCameraHeight = 1.63f;

    [MenuItem("Tools/Fix Main Camera Position")]
    public static void FixCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            var cameras = Object.FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
                mainCamera = cameras[0];
        }

        if (mainCamera == null)
        {
            Debug.LogError("No camera found in scene. Add a Camera or XR Origin with a camera.");
            return;
        }

        Transform cameraOffset = mainCamera.transform.parent;
        Transform xrOrigin = cameraOffset != null ? cameraOffset.parent : null;

        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        if (cameraOffset != null)
        {
            cameraOffset.localPosition = new Vector3(0f, DefaultCameraHeight, 0f);
            cameraOffset.localRotation = Quaternion.identity;
        }

        if (xrOrigin != null)
        {
            xrOrigin.localPosition = Vector3.zero;
            xrOrigin.localRotation = Quaternion.identity;
        }

        Debug.Log("XR camera reset: Main Camera local transform is zero, Camera Offset is 1.63m, and XR Origin is at the scene origin.");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
