using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent (DontDestroyOnLoad) controller for the multi-scene reserve-forces
// training course: 군장(Gear) -> 지뢰(Mine) -> 사격(Shooting) -> CQB -> 결과(Result).
//
// - One global timer that starts the first time the trainee enters any module
//   from the hall, and stops automatically once every module is complete.
// - Tracks per-module completion + pass/fail + the time each module finished at.
// - Free navigation (any module from the hall) plus ordered "next module".
// - Scores live in the existing TrainingScoreManager singleton; this manager
//   reads from it for the final results.
//
// Scene flow uses single-scene loads, so each feature scene keeps its own XR rig.
public class TrainingFlowManager : MonoBehaviour
{
    public static TrainingFlowManager Instance { get; private set; }

    [SerializeField] private string hallSceneName = "MainHall";
    [SerializeField] private string resultSceneName = "ResultScene";
    // Ordered modules used by GoToNext and by AllComplete/AllPassed.
    [SerializeField] private List<string> moduleScenes = new List<string>
    {
        "GearScene", "MineMap", "ShootingScene", "CQBScene"
    };

    private float startTime = -1f;       // Time.time when the global timer started
    private float frozenElapsed = -1f;   // total time, frozen once the course is done
    private readonly HashSet<string> completed = new HashSet<string>();
    private readonly Dictionary<string, bool> passed = new Dictionary<string, bool>();
    private readonly Dictionary<string, float> finishedAt = new Dictionary<string, float>();

    public IReadOnlyList<string> ModuleScenes => moduleScenes;
    public bool TimerRunning => startTime >= 0f && frozenElapsed < 0f;
    public float Elapsed => frozenElapsed >= 0f ? frozenElapsed : (startTime >= 0f ? Time.time - startTime : 0f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Starts the global timer on first module entry (idempotent).
    public void StartTimerIfNeeded()
    {
        if (startTime < 0f)
        {
            startTime = Time.time;
            frozenElapsed = -1f;
        }
    }

    // ---- Navigation ----
    public void GoToModule(string sceneName)
    {
        StartTimerIfNeeded();
        SceneManager.LoadScene(sceneName);
    }

    public void GoToHall() => SceneManager.LoadScene(hallSceneName);
    public void GoToResult() => SceneManager.LoadScene(resultSceneName);

    // Advance to the next module in order; if last, go to results.
    public void GoToNext(string currentScene)
    {
        int i = moduleScenes.IndexOf(currentScene);
        if (i >= 0 && i + 1 < moduleScenes.Count)
        {
            GoToModule(moduleScenes[i + 1]);
        }
        else
        {
            GoToResult();
        }
    }

    // ---- Module completion ----
    // A feature scene calls this when its module is finished (pass = met criteria).
    public void CompleteModule(string sceneName, bool pass)
    {
        completed.Add(sceneName);
        passed[sceneName] = pass;
        finishedAt[sceneName] = Elapsed;
        if (AllComplete())
        {
            FreezeTimer();
        }
    }

    public bool IsComplete(string sceneName) => completed.Contains(sceneName);
    public bool DidPass(string sceneName) => passed.TryGetValue(sceneName, out bool p) && p;
    public float FinishedAt(string sceneName) => finishedAt.TryGetValue(sceneName, out float t) ? t : -1f;

    public bool AllComplete()
    {
        foreach (var m in moduleScenes)
        {
            if (!completed.Contains(m)) return false;
        }
        return true;
    }

    public bool AllPassed()
    {
        foreach (var m in moduleScenes)
        {
            if (!(passed.TryGetValue(m, out bool p) && p)) return false;
        }
        return true;
    }

    private void FreezeTimer()
    {
        if (frozenElapsed < 0f && startTime >= 0f)
        {
            frozenElapsed = Time.time - startTime;
        }
    }

    // Full reset (e.g. for a "retry course" button).
    public void ResetCourse()
    {
        startTime = -1f;
        frozenElapsed = -1f;
        completed.Clear();
        passed.Clear();
        finishedAt.Clear();
    }

    // "PASS" / "RETRY" — ASCII-safe for the default TMP font.
    public string ResultText() => AllPassed() ? "PASS (수료)" : "RETRY (재이수)";

    public string ElapsedText()
    {
        float t = Elapsed;
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return string.Format("{0:00}:{1:00}", m, s);
    }
}
