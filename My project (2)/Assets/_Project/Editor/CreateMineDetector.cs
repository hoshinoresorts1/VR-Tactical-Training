using UnityEditor;
using UnityEngine;

public static class CreateMineDetector
{
    private const string DetectorModelPath = "Assets/Models/metal-detector/source/Metal Detector.fbx";

    [MenuItem("Tools/Mine/Create Mine Detector")]
    public static void CreateDetector()
    {
        GameObject existing = GameObject.Find("MineDetector");
        if (existing != null)
        {
            MineDetector mineDetector = existing.GetComponent<MineDetector>();
            if (mineDetector == null)
                mineDetector = existing.AddComponent<MineDetector>();

            if (Camera.main != null)
            {
                existing.transform.SetParent(Camera.main.transform, false);
                existing.transform.localPosition = new Vector3(0.55f, -0.55f, 1.2f);
                existing.transform.localRotation = Quaternion.identity;
                existing.transform.localScale = Vector3.one;
            }

            ConfigureDetector(mineDetector);
            mineDetector.SetDetectOnlyWhileHeld(false);
            EditorUtility.SetDirty(existing);
            Selection.activeGameObject = existing;
            EditorGUIUtility.PingObject(existing);
            return;
        }

        Transform parent = Camera.main != null ? Camera.main.transform : null;
        GameObject detector = new GameObject("MineDetector");
        detector.name = "MineDetector";
        detector.transform.SetParent(parent, false);
        detector.transform.localPosition = new Vector3(0.55f, -0.55f, 1.2f);
        detector.transform.localRotation = Quaternion.identity;
        detector.transform.localScale = Vector3.one;

        MineDetector newDetector = detector.AddComponent<MineDetector>();
        ConfigureDetector(newDetector);
        newDetector.SetDetectOnlyWhileHeld(false);

        Selection.activeGameObject = detector;
        EditorGUIUtility.PingObject(detector);
        EditorUtility.SetDirty(detector);
    }

    [MenuItem("Tools/Mine/Create Pickup Mine Detector")]
    public static void CreatePickupDetector()
    {
        GameObject existing = GameObject.Find("MineDetector");
        GameObject detector = existing != null ? existing : new GameObject("MineDetector");
        detector.name = "MineDetector";
        detector.transform.SetParent(null);

        if (Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            detector.transform.position = cameraTransform.position + cameraTransform.forward * 1.2f + Vector3.down * 0.35f;
            detector.transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        }
        else
        {
            detector.transform.position = Vector3.zero;
            detector.transform.rotation = Quaternion.identity;
        }

        detector.transform.localScale = Vector3.one;

        MineDetector mineDetector = detector.GetComponent<MineDetector>();
        if (mineDetector == null)
            mineDetector = detector.AddComponent<MineDetector>();

        ConfigureDetector(mineDetector);
        mineDetector.SetDetectOnlyWhileHeld(true);
        EditorUtility.SetDirty(detector);
        Selection.activeGameObject = detector;
        EditorGUIUtility.PingObject(detector);
    }

    private static void ConfigureDetector(MineDetector mineDetector)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(DetectorModelPath);
        if (model != null)
            mineDetector.SetDetectorModelPrefab(model);

        mineDetector.RebuildVisibleModel();
        mineDetector.EnsureGrabSetup();
    }
}
