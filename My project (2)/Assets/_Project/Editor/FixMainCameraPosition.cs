using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class FixMainCameraPosition
{
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

        mainCamera.transform.position = new Vector3(0f, 1.6f, -5f);
        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Debug.Log("Main Camera position fixed to (0, 1.6, -5) and rotation reset.");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
