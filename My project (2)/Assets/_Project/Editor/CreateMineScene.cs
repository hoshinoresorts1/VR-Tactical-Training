using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CreateMineScene
{
    [MenuItem("Tools/Create Mine Scene (Field)")]
    public static void CreateScene()
    {
        // Ensure scene folder
        string sceneFolder = "Assets/_Project/Scenes";
        if (!AssetDatabase.IsValidFolder(sceneFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        }

        // Ensure Mine tag exists
        AddTagIfMissing("Mine");

        // Create new empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create ground (plane)
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "FieldGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f); // larger field

        // Add simple terrain look (optional material)
        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Player start (empty)
        var player = new GameObject("PlayerStart");
        player.transform.position = new Vector3(0f, 1.5f, -8f);

        // DetectionManager
        var dmGO = new GameObject("DetectionManager");
        var dm = dmGO.AddComponent<DetectionManager>();
        dm.detectionRange = 10f;
        var audio = dmGO.AddComponent<AudioSource>();
        dm.audioSource = audio;

        // Create Canvas + UI (Slider + TMP Text)
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Detection UI container
        var uiGO = new GameObject("DetectionUI");
        uiGO.transform.SetParent(canvasGO.transform, false);
        var detectionUI = uiGO.AddComponent<DetectionUI>();

        // Slider
        var sliderGO = new GameObject("DetectionSlider");
        sliderGO.transform.SetParent(uiGO.transform, false);
        var slider = sliderGO.AddComponent<UnityEngine.UI.Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        detectionUI.detectionSlider = slider;

        // Text (uses TextMeshPro if available)
        UnityEngine.UI.Text legacyText = null;
        TMPro.TextMeshProUGUI tmpText = null;
        var textGO = new GameObject("DetectionPercent");
        textGO.transform.SetParent(uiGO.transform, false);
        // Try to add TMP first
        try
        {
            tmpText = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.text = "0%";
            detectionUI.percentText = tmpText as TMPro.TMP_Text;
        }
        catch
        {
            legacyText = textGO.AddComponent<UnityEngine.UI.Text>();
            legacyText.text = "0%";
            // Wrap legacy text into TMP.Text for compatibility field (will be null)
        }

        // Detector tool (placeholder) - an object representing handheld detector
        var detectorGO = new GameObject("DetectorTool");
        detectorGO.transform.position = player.transform.position + Vector3.forward * 1.5f;
        var detector = detectorGO.AddComponent<DetectorTool>();
        detector.detectionManager = dm;

        // Instantiate mine prefabs at three positions
        string prefabPath = "Assets/_Project/Prefabs/Mine/MineTrainingPrefab.prefab";
        GameObject minePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        Vector3[] positions = new Vector3[] { new Vector3(-6f, 0f, 3f), new Vector3(2f, 0f, 5f), new Vector3(6f, 0f, -2f) };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject mineGO;
            if (minePrefab != null)
            {
                mineGO = (GameObject)PrefabUtility.InstantiatePrefab(minePrefab);
            }
            else
            {
                mineGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mineGO.name = "Mine_Placeholder_" + i;
            }
            mineGO.transform.position = positions[i];
            mineGO.transform.SetParent(null);
        }

        // Save scene asset
        string scenePath = sceneFolder + "/MineScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MineScene created at: " + scenePath + ". Place VR rig and test detection/defusal.");
    }

    private static void AddTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) { found = true; break; }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log("Added tag: " + tag);
        }
    }
}
