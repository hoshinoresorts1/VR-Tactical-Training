using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class SetupXRNoHMD
{
    [MenuItem("Tools/Setup XR No HMD")] 
    public static void SetupXRScene()
    {
        string xrOriginPath = "Assets/Samples/XR Interaction Toolkit/3.1.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        string deviceSimulatorPath = "Assets/Samples/XR Interaction Toolkit/3.1.2/XR Device Simulator/XRDeviceSimulator/XR Device Simulator.prefab";

        var xrOriginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(xrOriginPath);
        var deviceSimulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(deviceSimulatorPath);

        if (xrOriginPrefab == null)
        {
            Debug.LogError("XR Origin prefab not found at: " + xrOriginPath + ". Please import the XR Interaction Toolkit samples.");
        }
        else
        {
            PrefabUtility.InstantiatePrefab(xrOriginPrefab);
        }

        if (deviceSimulatorPrefab == null)
        {
            Debug.LogError("XR Device Simulator prefab not found at: " + deviceSimulatorPath + ". Please import the XR Device Simulator sample.");
        }
        else
        {
            PrefabUtility.InstantiatePrefab(deviceSimulatorPrefab);
        }

        Debug.Log("XR No HMD setup completed. Check Hierarchy for XR Origin and XR Device Simulator.");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
