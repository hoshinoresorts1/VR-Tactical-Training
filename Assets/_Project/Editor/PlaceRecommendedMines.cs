using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlaceRecommendedMines
{
    private const string GenericMinePath = "Assets/Models/generic_mine/scene.gltf";
    private const string MineBoxPath = "Assets/Models/mine-box-full-pipeline/source/Mine_box.fbx";
    private static readonly Vector3[] RecommendedPositions =
    {
        new Vector3(-9f, 0.05f, -10f),
        new Vector3(-8f, 0.05f, -10f),
        new Vector3(-6f, 0.05f, -9f),
        new Vector3(-7f, 0.05f, -8f),
        new Vector3(-9f, 0.05f, -7f),
        new Vector3(-4f, 0.05f, -8f),
        new Vector3(-5f, 0.05f, -5f),
        new Vector3(-7f, 0.05f, -4f),
        new Vector3(-3f, 0.05f, -6f),
        new Vector3(-2f, 0.05f, -4f),
        new Vector3(-4f, 0.05f, -2f),
        new Vector3(0f, 0.05f, -5f),
        new Vector3(0f, 0.05f, -1f),
        new Vector3(-1f, 0.05f, -2f),
        new Vector3(2f, 0.05f, -2f),
        new Vector3(2f, 0.05f, 1f),
        new Vector3(1f, 0.05f, 2f),
        new Vector3(3f, 0.05f, 0f),
        new Vector3(4f, 0.05f, 3f),
        new Vector3(3f, 0.05f, 4f),
        new Vector3(5f, 0.05f, 1f),
        new Vector3(6f, 0.05f, 5f),
        new Vector3(5f, 0.05f, 6f),
        new Vector3(7f, 0.05f, 4f),
        new Vector3(8f, 0.05f, 7f),
        new Vector3(7f, 0.05f, 8f),
        new Vector3(9f, 0.05f, 6f),
        new Vector3(10f, 0.05f, 9f),
        new Vector3(9f, 0.05f, 10f),
        new Vector3(11f, 0.05f, 8f),
        new Vector3(-10f, 0.05f, -5f),
        new Vector3(-11f, 0.05f, -2f),
        new Vector3(-8f, 0.05f, -1f),
        new Vector3(-5f, 0.05f, 2f),
        new Vector3(-7f, 0.05f, 4f),
        new Vector3(-3f, 0.05f, 3f),
        new Vector3(1f, 0.05f, 7f),
        new Vector3(-1f, 0.05f, 9f),
        new Vector3(3f, 0.05f, 9f),
        new Vector3(7f, 0.05f, -1f),
        new Vector3(9f, 0.05f, -3f),
        new Vector3(5f, 0.05f, -3f),
        new Vector3(11f, 0.05f, 2f),
        new Vector3(10f, 0.05f, 4f),
        new Vector3(12f, 0.05f, -1f),
        new Vector3(-12f, 0.05f, -8f),
        new Vector3(-11f, 0.05f, -11f),
        new Vector3(-10f, 0.05f, 1f),
        new Vector3(-9f, 0.05f, 6f),
        new Vector3(-8f, 0.05f, 10f),
        new Vector3(-6f, 0.05f, -11f),
        new Vector3(-6f, 0.05f, 7f),
        new Vector3(-4f, 0.05f, 11f),
        new Vector3(-2f, 0.05f, 5f),
        new Vector3(-1f, 0.05f, -8f),
        new Vector3(1f, 0.05f, -10f),
        new Vector3(2f, 0.05f, 11f),
        new Vector3(4f, 0.05f, -7f),
        new Vector3(5f, 0.05f, 10f),
        new Vector3(6f, 0.05f, -5f),
        new Vector3(8f, 0.05f, 1f),
        new Vector3(8f, 0.05f, 11f),
        new Vector3(10f, 0.05f, -6f),
        new Vector3(11f, 0.05f, -9f),
        new Vector3(12f, 0.05f, 6f),
        new Vector3(-12f, 0.05f, 4f),
        new Vector3(0f, 0.05f, 11f),
        new Vector3(12f, 0.05f, 11f),
    };

    private static readonly Vector3[] FillGapPositions =
    {
        new Vector3(-12f, 0.05f, -2f),
        new Vector3(-12f, 0.05f, 8f),
        new Vector3(-10f, 0.05f, -9f),
        new Vector3(-10f, 0.05f, 10f),
        new Vector3(-9f, 0.05f, 3f),
        new Vector3(-8f, 0.05f, -6f),
        new Vector3(-7f, 0.05f, 1f),
        new Vector3(-6f, 0.05f, -1f),
        new Vector3(-5f, 0.05f, 5f),
        new Vector3(-5f, 0.05f, 9f),
        new Vector3(-4f, 0.05f, -10f),
        new Vector3(-3f, 0.05f, 0f),
        new Vector3(-3f, 0.05f, 7f),
        new Vector3(-2f, 0.05f, -11f),
        new Vector3(-1f, 0.05f, 1f),
        new Vector3(0f, 0.05f, -9f),
        new Vector3(0f, 0.05f, 5f),
        new Vector3(1f, 0.05f, -6f),
        new Vector3(2f, 0.05f, 6f),
        new Vector3(3f, 0.05f, -10f),
        new Vector3(4f, 0.05f, 7f),
        new Vector3(5f, 0.05f, -9f),
        new Vector3(6f, 0.05f, 0f),
        new Vector3(6f, 0.05f, 8f),
        new Vector3(7f, 0.05f, -7f),
        new Vector3(8f, 0.05f, -11f),
        new Vector3(9f, 0.05f, 3f),
        new Vector3(10f, 0.05f, -1f),
        new Vector3(11f, 0.05f, -4f),
        new Vector3(11f, 0.05f, 6f),
    };

    [MenuItem("Tools/Mine/Place Recommended Mines")]
    public static void PlaceMines()
    {
        PlaceMinesFromPositions(RecommendedPositions, "recommended");
    }

    [MenuItem("Tools/Mine/Place Half Recommended Mines")]
    public static void PlaceHalfMines()
    {
        int halfCount = Mathf.CeilToInt(RecommendedPositions.Length * 0.5f);
        Vector3[] halfPositions = new Vector3[halfCount];
        for (int i = 0; i < halfCount; i++)
            halfPositions[i] = RecommendedPositions[i * 2];

        PlaceMinesFromPositions(halfPositions, "half recommended");
    }

    [MenuItem("Tools/Mine/Add Gap Fill Mines")]
    public static void AddGapFillMines()
    {
        AddTagIfMissing("Mine");

        GameObject container = GameObject.Find("RecommendedMines");
        if (container == null)
        {
            container = new GameObject("RecommendedMines");
            Undo.RegisterCreatedObjectUndo(container, "Create RecommendedMines");
        }

        GameObject visualAsset = LoadVisualAsset();
        int created = 0;
        int startIndex = Object.FindObjectsOfType<Mine>(true).Length;

        for (int i = 0; i < FillGapPositions.Length; i++)
        {
            if (HasMineNear(FillGapPositions[i], 0.65f))
                continue;

            CreateMine(container.transform, FillGapPositions[i], startIndex + created + 1, visualAsset);
            created++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = container;
        EditorGUIUtility.PingObject(container);

        Debug.Log($"Added {created} gap-fill mines. Existing mines were kept.");
    }

    private static void PlaceMinesFromPositions(Vector3[] positions, string label)
    {
        AddTagIfMissing("Mine");
        ClearExistingMines();

        GameObject container = new GameObject("RecommendedMines");
        Undo.RegisterCreatedObjectUndo(container, "Create RecommendedMines");

        GameObject visualAsset = LoadVisualAsset();
        for (int i = 0; i < positions.Length; i++)
        {
            CreateMine(container.transform, positions[i], i + 1, visualAsset);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = container;
        EditorGUIUtility.PingObject(container);

        Debug.Log($"Placed {positions.Length} {label} mines.");
    }

    private static void ClearExistingMines()
    {
        Mine[] mines = Object.FindObjectsOfType<Mine>(true);
        foreach (Mine mine in mines)
        {
            if (mine != null)
                Undo.DestroyObjectImmediate(mine.gameObject);
        }

        Transform[] transforms = Object.FindObjectsOfType<Transform>(true);
        foreach (Transform transform in transforms)
        {
            if (transform == null)
                continue;

            if (transform.name.StartsWith("mine1"))
                Undo.DestroyObjectImmediate(transform.gameObject);
        }

        GameObject existingContainer = GameObject.Find("RecommendedMines");
        if (existingContainer != null)
            Undo.DestroyObjectImmediate(existingContainer);
    }

    private static GameObject LoadVisualAsset()
    {
        GameObject visualAsset = AssetDatabase.LoadAssetAtPath<GameObject>(GenericMinePath);
        if (visualAsset != null)
            return visualAsset;

        return AssetDatabase.LoadAssetAtPath<GameObject>(MineBoxPath);
    }

    private static void CreateMine(Transform container, Vector3 position, int index, GameObject visualAsset)
    {
        GameObject mineObject = new GameObject($"mine1 ({index})");
        Undo.RegisterCreatedObjectUndo(mineObject, "Create Mine");
        mineObject.transform.SetParent(container, true);
        mineObject.transform.position = position;
        mineObject.tag = "Mine";

        Mine mine = mineObject.AddComponent<Mine>();
        mine.startsHidden = true;
        mine.triggerRadius = 0.45f;
        mine.interactionDistance = 3f;

        GameObject visual = CreateVisual(visualAsset);
        visual.name = "scene";
        visual.transform.SetParent(mineObject.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        visual.transform.localScale = Vector3.one * 0.08f;
    }

    private static bool HasMineNear(Vector3 position, float minDistance)
    {
        Mine[] mines = Object.FindObjectsOfType<Mine>(true);
        foreach (Mine mine in mines)
        {
            if (mine == null)
                continue;

            Vector2 mineXZ = new Vector2(mine.transform.position.x, mine.transform.position.z);
            Vector2 positionXZ = new Vector2(position.x, position.z);
            if (Vector2.Distance(mineXZ, positionXZ) < minDistance)
                return true;
        }

        return false;
    }

    private static GameObject CreateVisual(GameObject visualAsset)
    {
        if (visualAsset != null)
        {
            Object instance = PrefabUtility.InstantiatePrefab(visualAsset);
            if (instance is GameObject visualObject)
                return visualObject;
        }

        GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fallback.transform.localScale = new Vector3(1.8f, 0.18f, 1.8f);

        Renderer renderer = fallback.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            if (shader != null)
                renderer.sharedMaterial = new Material(shader) { color = Color.black };
        }

        return fallback;
    }

    private static void AddTagIfMissing(string tag)
    {
        Object[] tagAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (tagAssets == null || tagAssets.Length == 0)
            return;

        SerializedObject tagManager = new SerializedObject(tagAssets[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
