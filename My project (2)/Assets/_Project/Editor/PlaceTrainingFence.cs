using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlaceTrainingFence
{
    private const string FenceAssetPath = "Assets/Models/barbed-wire_fence/scene.gltf";
    private const string ContainerName = "TrainingFence";

    private const float MinX = -14.5f;
    private const float MaxX = 14.5f;
    private const float MinZ = -14.5f;
    private const float MaxZ = 14.5f;
    private const float FenceY = 0f;
    private const float GapRatio = 0.96f;
    private const float UprightXRotation = -90f;

    private static readonly Vector3 FenceScale = Vector3.one * 0.085f;

    [MenuItem("Tools/Mine/Place Barbed Wire Fence")]
    public static void PlaceFence()
    {
        ClearExistingFence();

        GameObject fenceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FenceAssetPath);
        if (fenceAsset == null)
        {
            Debug.LogError("Barbed wire fence asset not found: " + FenceAssetPath);
            return;
        }

        GameObject container = new GameObject(ContainerName);
        Undo.RegisterCreatedObjectUndo(container, "Create Training Fence");

        int created = 0;
        float xYaw = FindYawForAxis(fenceAsset, Vector3.right);
        float zYaw = FindYawForAxis(fenceAsset, Vector3.forward);
        float xSegmentLength = Mathf.Max(0.5f, MeasureLength(fenceAsset, xYaw, Vector3.right) * GapRatio);
        float zSegmentLength = Mathf.Max(0.5f, MeasureLength(fenceAsset, zYaw, Vector3.forward) * GapRatio);

        created += PlaceLine(container.transform, fenceAsset, new Vector3(MinX, FenceY, MinZ), new Vector3(MaxX, FenceY, MinZ), xYaw, xSegmentLength, "South");
        created += PlaceLine(container.transform, fenceAsset, new Vector3(MinX, FenceY, MaxZ), new Vector3(MaxX, FenceY, MaxZ), xYaw + 180f, xSegmentLength, "North");
        created += PlaceLine(container.transform, fenceAsset, new Vector3(MinX, FenceY, MinZ), new Vector3(MinX, FenceY, MaxZ), zYaw, zSegmentLength, "West");
        created += PlaceLine(container.transform, fenceAsset, new Vector3(MaxX, FenceY, MinZ), new Vector3(MaxX, FenceY, MaxZ), zYaw + 180f, zSegmentLength, "East");

        Selection.activeGameObject = container;
        EditorGUIUtility.PingObject(container);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"Placed {created} barbed wire fence segments around the mine map.");
    }

    [MenuItem("Tools/Mine/Clear Barbed Wire Fence")]
    public static void ClearFenceMenu()
    {
        ClearExistingFence();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static int PlaceLine(Transform parent, GameObject fenceAsset, Vector3 start, Vector3 end, float yRotation, float segmentLength, string sideName)
    {
        float length = Vector3.Distance(start, end);
        int count = Mathf.Max(1, Mathf.CeilToInt(length / segmentLength));
        int created = 0;

        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.5f) / count;
            Vector3 position = Vector3.Lerp(start, end, t);
            GameObject segment = CreateFenceSegment(fenceAsset, position, CreateFenceRotation(yRotation), $"{sideName}_bar_{i + 1:00}");
            segment.transform.SetParent(parent, true);
            created++;
        }

        return created;
    }

    private static float FindYawForAxis(GameObject fenceAsset, Vector3 axis)
    {
        float bestYaw = 0f;
        float bestLength = -1f;
        float[] yaws = { 0f, 90f, 180f, 270f };

        foreach (float yaw in yaws)
        {
            float length = MeasureLength(fenceAsset, yaw, axis);
            if (length > bestLength)
            {
                bestLength = length;
                bestYaw = yaw;
            }
        }

        return bestYaw;
    }

    private static float MeasureLength(GameObject fenceAsset, float yRotation, Vector3 axis)
    {
        GameObject sample = Object.Instantiate(fenceAsset);
        sample.hideFlags = HideFlags.HideAndDontSave;
        sample.transform.position = Vector3.zero;
        sample.transform.rotation = CreateFenceRotation(yRotation);
        sample.transform.localScale = FenceScale;

        Bounds bounds = CalculateBounds(sample);
        Object.DestroyImmediate(sample);

        axis = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
        return Vector3.Dot(bounds.size, axis);
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(root.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static Quaternion CreateFenceRotation(float yRotation)
    {
        return Quaternion.Euler(UprightXRotation, yRotation, 0f);
    }

    private static GameObject CreateFenceSegment(GameObject fenceAsset, Vector3 position, Quaternion rotation, string name)
    {
        Object instance = PrefabUtility.InstantiatePrefab(fenceAsset);
        GameObject segment = instance as GameObject;
        if (segment == null)
        {
            segment = Object.Instantiate(fenceAsset);
            Undo.RegisterCreatedObjectUndo(segment, "Create Fence Segment");
        }
        else
        {
            Undo.RegisterCreatedObjectUndo(segment, "Create Fence Segment");
        }

        segment.name = name;
        segment.transform.position = position;
        segment.transform.rotation = rotation;
        segment.transform.localScale = FenceScale;
        AlignBottomToGround(segment);
        return segment;
    }

    private static void AlignBottomToGround(GameObject segment)
    {
        Bounds bounds = CalculateBounds(segment);
        float yOffset = FenceY - bounds.min.y;
        segment.transform.position += Vector3.up * yOffset;
    }

    private static void ClearExistingFence()
    {
        GameObject existing = GameObject.Find(ContainerName);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }
}
