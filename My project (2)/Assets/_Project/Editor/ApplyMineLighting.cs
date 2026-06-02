using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ApplyMineLighting
{
    [MenuItem("Tools/Mine/Apply Dark Training Lighting")]
    public static void ApplyDarkTrainingLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.24f, 0.26f, 0.28f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.42f, 0.45f, 0.46f);
        RenderSettings.fogDensity = 0.012f;

        Light sun = FindSunLight();
        if (sun == null)
        {
            GameObject sunObject = new GameObject("Training Sun");
            Undo.RegisterCreatedObjectUndo(sunObject, "Create Training Sun");
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        Undo.RecordObject(sun, "Apply Mine Lighting");
        sun.name = "Training Sun";
        sun.type = LightType.Directional;
        sun.intensity = 0.55f;
        sun.color = new Color(0.86f, 0.9f, 1f);
        sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Applied darker mine training lighting.");
    }

    [MenuItem("Tools/Mine/Apply Bright Test Lighting")]
    public static void ApplyBrightTestLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.6f);
        RenderSettings.fog = false;

        Light sun = FindSunLight();
        if (sun != null)
        {
            Undo.RecordObject(sun, "Apply Bright Mine Lighting");
            sun.intensity = 1f;
            sun.color = Color.white;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Applied brighter test lighting.");
    }

    private static Light FindSunLight()
    {
        Light[] lights = Object.FindObjectsOfType<Light>(true);
        foreach (Light light in lights)
        {
            if (light != null && light.type == LightType.Directional)
                return light;
        }

        return null;
    }
}
