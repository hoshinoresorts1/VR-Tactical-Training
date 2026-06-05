using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public static class CreateMineTrainingPrefab
{
    private const string PrefabDirectory = "Assets/_Project/Prefabs/Mine";
    private const string PrefabPath = PrefabDirectory + "/MineTrainingArea.prefab";
    private const string RootName = "MineTrainingArea";

    [MenuItem("Tools/Mine/Create Mine Training Area Prefab")]
    public static void CreatePrefab()
    {
        EnsurePrefabDirectory();

        GameObject root = new GameObject(RootName);
        try
        {
            foreach (GameObject source in FindTrainingObjects())
            {
                GameObject copy = Object.Instantiate(source);
                copy.name = source.name;
                copy.transform.SetParent(root.transform, true);
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created mine training area prefab: " + PrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static IEnumerable<GameObject> FindTrainingObjects()
    {
        HashSet<GameObject> results = new HashSet<GameObject>();

        AddByName(results, "RecommendedMines");
        AddByName(results, "TrainingFence");
        AddByName(results, "MineGoal");
        AddByName(results, "MineDetector");

        AddComponentRoots<Mine>(results);
        AddComponentRoots<MineGoal>(results);
        AddComponentRoots<MineDetector>(results);

        foreach (GameObject root in Object.FindObjectsOfType<GameObject>(true))
        {
            if (root.transform.parent != null)
                continue;

            if (ShouldIncludeEnvironmentRoot(root))
                results.Add(root);
        }

        results.RemoveWhere(ShouldExclude);
        return results;
    }

    private static void AddByName(HashSet<GameObject> results, string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        if (found != null)
            results.Add(GetTopTrainingRoot(found));
    }

    private static void AddComponentRoots<T>(HashSet<GameObject> results) where T : Component
    {
        T[] components = Object.FindObjectsOfType<T>(true);
        foreach (T component in components)
        {
            if (component != null)
                results.Add(GetTopTrainingRoot(component.gameObject));
        }
    }

    private static GameObject GetTopTrainingRoot(GameObject obj)
    {
        Transform current = obj.transform;
        while (current.parent != null && !ShouldExclude(current.parent.gameObject))
        {
            current = current.parent;
        }

        return current.gameObject;
    }

    private static bool ShouldIncludeEnvironmentRoot(GameObject obj)
    {
        string name = obj.name.ToLowerInvariant();
        return name.Contains("plane")
            || name.Contains("terrain")
            || name.Contains("ground")
            || name.Contains("flag")
            || name.Contains("tree")
            || name.Contains("bar")
            || name.Contains("fence");
    }

    private static bool ShouldExclude(GameObject obj)
    {
        if (obj == null)
            return true;

        string name = obj.name.ToLowerInvariant();
        if (name.Contains("xr origin") || name.Contains("xr rig"))
            return true;

        if (name.Contains("eventsystem") || name.Contains("camera") || name.Contains("directional light"))
            return true;

        if (obj.GetComponent<Camera>() != null || obj.GetComponent<EventSystem>() != null)
            return true;

        return false;
    }

    private static void EnsurePrefabDirectory()
    {
        if (!Directory.Exists(PrefabDirectory))
            Directory.CreateDirectory(PrefabDirectory);
    }
}
