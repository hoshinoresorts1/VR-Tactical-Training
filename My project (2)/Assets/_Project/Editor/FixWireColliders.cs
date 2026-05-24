using UnityEditor;
using UnityEngine;

public static class FixWireColliders
{
    [MenuItem("Tools/Fix Mine Wire Colliders")] 
    public static void FixColliders()
    {
        string folder = "Assets/_Project/Prefabs/Mine";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        int fixedCount = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefabRoot = PrefabUtility.LoadPrefabContents(path);
            if (prefabRoot == null) continue;
            var wires = prefabRoot.GetComponentsInChildren<Collider>(true);
            foreach (var c in wires)
            {
                if (c.gameObject.name.StartsWith("Wire_"))
                {
                    if (c.isTrigger)
                    {
                        c.isTrigger = false;
                        fixedCount++;
                    }
                }
            }
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
        Debug.Log($"Fixed {fixedCount} wire colliders in prefabs under: {folder}");
    }
}
