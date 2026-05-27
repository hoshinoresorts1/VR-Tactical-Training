using UnityEditor;
using UnityEngine;

public static class CreateMineDetector
{
    [MenuItem("Tools/Mine/Create Mine Detector")]
    public static void CreateDetector()
    {
        GameObject existing = GameObject.Find("MineDetector");
        if (existing != null)
        {
            MineDetector mineDetector = existing.GetComponent<MineDetector>();
            if (mineDetector == null)
                existing.AddComponent<MineDetector>();

            if (Camera.main != null && existing.transform.parent == null)
            {
                existing.transform.SetParent(Camera.main.transform, false);
                existing.transform.localPosition = new Vector3(0.55f, -0.55f, 1.2f);
                existing.transform.localRotation = Quaternion.identity;
                existing.transform.localScale = Vector3.one;
            }

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

        detector.AddComponent<MineDetector>();

        Selection.activeGameObject = detector;
        EditorGUIUtility.PingObject(detector);
        EditorUtility.SetDirty(detector);
    }
}
