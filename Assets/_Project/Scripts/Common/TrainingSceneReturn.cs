using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingSceneReturn : MonoBehaviour
{
    public static TrainingSceneReturn Instance { get; private set; }

    [Tooltip("Main hub scene name. Must be in Build Settings.")]
    public string mainHallSceneName = "SampleScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureManagerOnSceneLoad()
    {
        GetOrCreate();
    }

    public static TrainingSceneReturn GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("TrainingSceneReturn");
        return managerObject.AddComponent<TrainingSceneReturn>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ReturnToMainHall()
    {
        if (string.IsNullOrWhiteSpace(mainHallSceneName))
        {
            Debug.LogWarning("Main hall scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainHallSceneName))
        {
            Debug.LogWarning("Main hall scene is not in Build Settings yet: " + mainHallSceneName);
            return;
        }

        SceneManager.LoadScene(mainHallSceneName);
    }
}
