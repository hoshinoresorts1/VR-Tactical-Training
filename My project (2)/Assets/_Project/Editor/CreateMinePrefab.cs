using UnityEngine;
using UnityEditor;

public static class CreateMinePrefab
{
    [MenuItem("Tools/Create Mine Prefab (Field)")]
    public static void CreatePrefab()
    {
        // Ensure folder exists
        string prefabFolder = "Assets/_Project/Prefabs/Mine";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Mine");
        }

        // Root
        GameObject root = new GameObject("MinePrefab");
        var mine = root.AddComponent<Mine>();

        // Defusal root
        GameObject defusalRoot = new GameObject("Defusal");
        defusalRoot.transform.SetParent(root.transform, false);
        var wireDef = defusalRoot.AddComponent<WireDefusal>();

        // Wires group
        GameObject wires = new GameObject("Wires");
        wires.transform.SetParent(defusalRoot.transform, false);

        Color[] colors = new Color[] { Color.red, Color.green, Color.blue };
        for (int i = 0; i < 3; i++)
        {
            GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wire.name = "Wire_" + i;
            wire.transform.SetParent(wires.transform, false);
            wire.transform.localScale = new Vector3(0.02f, 0.25f, 0.02f);
            wire.transform.localPosition = new Vector3((i - 1) * 0.06f, 0f, 0f);

            var mr = wire.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard")) { color = colors[i] };
            mr.sharedMaterial = mat;

            // Add interactable
            var inter = wire.AddComponent<WireInteractable>();
            inter.wireIndex = i;
            inter.wireDefusal = wireDef;

            // Remove collider trigger to avoid physics issues
            var col = wire.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        // Assign defusal prefab reference on Mine
        // We'll make the defusalRoot a prefab and assign it
        string defusalPrefabPath = prefabFolder + "/WireDefusal.prefab";
        var defusalPrefab = PrefabUtility.SaveAsPrefabAsset(defusalRoot, defusalPrefabPath);
        if (defusalPrefab != null)
        {
            mine.defusalPrefab = defusalPrefab;
        }

        // Save full Mine prefab
        string prefabPath = prefabFolder + "/MineTrainingPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

        // Cleanup
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Mine prefab created at: " + prefabPath);
    }
}
