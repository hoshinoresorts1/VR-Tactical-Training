using UnityEngine;
using UnityEngine.SceneManagement;

// A "door": when the player's head (Camera.main) moves within triggerRadius of
// this object, the target scene loads immediately. No Player tag or rig changes
// needed — it polls the camera position. Place a visible door mesh on the same
// object so the player can see where to go.
public class SceneDoor : MonoBehaviour
{
    [Tooltip("Scene name to load (must be in Build Settings).")]
    public string targetSceneName;

    [Tooltip("How close the player's head must get (meters, horizontal).")]
    public float triggerRadius = 0.9f;

    [Tooltip("Seconds the player must stay inside before it loads (avoids accidental passes). 0 = instant.")]
    public float dwell = 0.0f;

    private bool loading;
    private float inside;

    private void Update()
    {
        if (loading) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 a = cam.transform.position; a.y = 0f;
        Vector3 b = transform.position;     b.y = 0f;

        if (Vector3.Distance(a, b) <= triggerRadius)
        {
            inside += Time.deltaTime;
            if (inside >= dwell) Load();
        }
        else
        {
            inside = 0f;
        }
    }

    public void Load()
    {
        if (loading) return;
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("SceneDoor targetSceneName is empty.", this);
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogWarning("Scene not in Build Settings: " + targetSceneName, this);
            return;
        }

        if (SceneManager.GetActiveScene().name == "MineMap")
        {
            MineDamageManager.GetOrCreate().CompleteTraining();
        }

        loading = true;
        SceneManager.LoadScene(targetSceneName);
    }
}
