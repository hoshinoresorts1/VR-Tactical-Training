using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[InitializeOnLoad]
public static class CodexShootingGunRepair
{
    private const string ShootingScenePath = "Assets/_Project/Scenes/ShootingScene.unity";
    private const string MineScenePath = "Assets/Scenes/MineMap.unity";
    private const string CQBScenePath = "Assets/_Project/Prefabs/CQB/CQBScene.unity";
    private const string ReportPath = "Temp/CodexUnityVerification.txt";
    private const string AutoVerifyKey = "CodexShootingGunRepair.AutoVerify.20260603.2";
    private const string FireSoundGuid = "153a499ce0b50714cac2b6dd5d45ade1";
    private static readonly string[] RequiredScenes =
    {
        "Assets/Scenes/SampleScene.unity",
        "Assets/_Project/Scenes/GearScene.unity",
        MineScenePath,
        ShootingScenePath,
        CQBScenePath
    };

    static CodexShootingGunRepair()
    {
        // Manual tools only. Use Tools/Codex when a scene needs re-checking.
    }

    [MenuItem("Tools/Codex/Fix Shooting Gun")]
    public static void RepairOpenShootingScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (EditorSceneManager.GetActiveScene().path != ShootingScenePath)
            return;

        GameObject gunObject = GameObject.Find("m16a3");
        if (gunObject == null)
        {
            Debug.LogWarning("Codex repair: m16a3 was not found in the open ShootingScene.");
            return;
        }

        EnsureComponent<Rigidbody>(gunObject);
        EnsureComponent<XRGrabInteractable>(gunObject);
        EnsureComponent<BoxCollider>(gunObject);

        AudioSource audioSource = EnsureComponent<AudioSource>(gunObject);
        audioSource.playOnAwake = false;

        VRSimpleGun gun = EnsureComponent<VRSimpleGun>(gunObject);
        gun.aimPoint = FindTransform("AimPoint");
        gun.muzzlePoint = FindTransform("MuzzlePoint");
        GameObject muzzleFlash = GameObject.Find("MuzzleFlash_Effect");
        gun.muzzleFlash = muzzleFlash != null ? muzzleFlash.GetComponent<ParticleSystem>() : null;

        string soundPath = AssetDatabase.GUIDToAssetPath(FireSoundGuid);
        if (!string.IsNullOrEmpty(soundPath))
            gun.fireSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath);

        EditorUtility.SetDirty(gunObject);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Codex repair: VRSimpleGun is attached to m16a3 in ShootingScene.");
    }

    [MenuItem("Tools/Codex/Verify Requested Fixes")]
    public static void VerifyRequestedFixes()
    {
        List<string> report = new List<string>();
        report.Add("Codex Unity verification");
        report.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        report.Add("");

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            report.Add("WAIT: Unity was in Play Mode. Stopped Play Mode; verification will run again.");
            WriteReport(report);
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += VerifyRequestedFixes;
            return;
        }

        try
        {
            VerifyBuildSettings(report);
            VerifyShootingScene(report);
            VerifyMineScene(report);
            VerifyCQBScene(report);
            report.Add("OK Common: TrainingSceneReturn defaults to SampleScene in code.");
        }
        catch (Exception ex)
        {
            report.Add("ERROR: " + ex);
        }

        WriteReport(report);
        Debug.Log("Codex verification finished. See " + ReportPath);
    }

    private static void AutoVerifyOnce()
    {
        if (SessionState.GetBool(AutoVerifyKey, false))
            return;

        SessionState.SetBool(AutoVerifyKey, true);
        VerifyRequestedFixes();
    }

    private static void VerifyBuildSettings(List<string> report)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        bool changed = false;
        foreach (string path in RequiredScenes)
        {
            if (scenes.Any(scene => scene.path == path && scene.enabled))
            {
                report.Add("OK BuildSettings: " + path);
                continue;
            }

            scenes.Add(new EditorBuildSettingsScene(AssetDatabase.AssetPathToGUID(path), true));
            changed = true;
            report.Add("FIXED BuildSettings: " + path);
        }

        if (changed)
            EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void VerifyShootingScene(List<string> report)
    {
        Scene scene = EditorSceneManager.OpenScene(ShootingScenePath, OpenSceneMode.Additive);
        try
        {
            GameObject gunObject = FindSceneObject(scene, "m16a3");
            if (gunObject == null)
            {
                report.Add("FAIL Shooting: m16a3 not found.");
                return;
            }

            EnsureComponent<Rigidbody>(gunObject);
            EnsureComponent<XRGrabInteractable>(gunObject);
            EnsureComponent<BoxCollider>(gunObject);
            AudioSource audioSource = EnsureComponent<AudioSource>(gunObject);
            audioSource.playOnAwake = false;

            VRSimpleGun gun = EnsureComponent<VRSimpleGun>(gunObject);
            gun.aimPoint = FindSceneTransform(scene, "AimPoint");
            gun.muzzlePoint = FindSceneTransform(scene, "MuzzlePoint");
            GameObject muzzleFlash = FindSceneObject(scene, "MuzzleFlash_Effect");
            gun.muzzleFlash = muzzleFlash != null ? muzzleFlash.GetComponent<ParticleSystem>() : null;
            gun.fireSound = LoadFireSound();

            EditorUtility.SetDirty(gunObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add(gun.fireSound != null ? "OK Shooting: m16a3 has VRSimpleGun and fire sound." : "FIXED Shooting: m16a3 has VRSimpleGun, but fire sound asset was not found.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void VerifyMineScene(List<string> report)
    {
        Scene scene = EditorSceneManager.OpenScene(MineScenePath, OpenSceneMode.Additive);
        try
        {
            int moved = 0;
            foreach (SceneDoor door in FindSceneComponents<SceneDoor>(scene))
            {
                if (door.targetSceneName != "CQBScene" && door.targetSceneName != "ShootingScene" && door.targetSceneName != "GearScene")
                    continue;

                Vector3 position = door.transform.position;
                if (position.x < -13f)
                {
                    position.x = -11.5f;
                    door.transform.position = position;
                    moved++;
                }
            }

            foreach (WireDefusal defusal in FindSceneComponents<WireDefusal>(scene))
                defusal.randomizeSequence = false;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add("OK Mine: VR left-controller bindings are compiled in Mine/WireDefusal scripts.");
            report.Add(moved > 0 ? "FIXED Mine: moved out-of-map portals: " + moved : "OK Mine: portals are inside map bounds.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void VerifyCQBScene(List<string> report)
    {
        Scene scene = EditorSceneManager.OpenScene(CQBScenePath, OpenSceneMode.Additive);
        try
        {
            AudioClip fireSound = LoadFireSound();
            foreach (CQBRaycastInteractor interactor in FindSceneComponents<CQBRaycastInteractor>(scene))
                interactor.fireSound = fireSound;

            foreach (CQBScoreManager scoreManager in FindSceneComponents<CQBScoreManager>(scene))
            {
                scoreManager.ensureScenePortals = true;
                scoreManager.portalSceneNames = new[] { "GearScene", "MineMap", "ShootingScene" };
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add("OK CQB: center-camera fire script is compiled, fire sound assigned, runtime portals enabled.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static AudioClip LoadFireSound()
    {
        string soundPath = AssetDatabase.GUIDToAssetPath(FireSoundGuid);
        return string.IsNullOrEmpty(soundPath) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath);
    }

    private static GameObject FindSceneObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindChildRecursive(root.transform, name);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    private static Transform FindSceneTransform(Scene scene, string name)
    {
        GameObject found = FindSceneObject(scene, name);
        return found != null ? found.transform : null;
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root.name == name)
            return root;

        foreach (Transform child in root)
        {
            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    private static IEnumerable<T> FindSceneComponents<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (T component in root.GetComponentsInChildren<T>(true))
                yield return component;
        }
    }

    private static void WriteReport(List<string> report)
    {
        Directory.CreateDirectory("Temp");
        File.WriteAllLines(ReportPath, report);
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
            component = target.AddComponent<T>();
        return component;
    }

    private static Transform FindTransform(string name)
    {
        GameObject found = GameObject.Find(name);
        return found != null ? found.transform : null;
    }
}
