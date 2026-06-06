using System.Collections;
using UnityEngine;

public class MineDamageManager : MonoBehaviour
{
    public static MineDamageManager Instance { get; private set; }

    [Header("Scoring")]
    public float fullScoreTime = 60f;
    public float penaltyInterval = 20f;
    public int penaltyPerInterval = 10;
    public int defusedMineBonus = 3;
    public int mineHitPenalty = 5;

    [Header("Hit Feedback")]
    public float knockbackDistance = 0.35f;
    public float verticalKick = 0f;
    public float knockbackDuration = 0.25f;
    public float messageDuration = 1.2f;
    public AudioSource audioSource;

    private bool trainingCleared;
    private string centerMessage = "";
    private float messageUntil;
    private AudioClip explosionClip;
    private Coroutine knockbackRoutine;
    private float trainingStartTime;
    private int defusedMineCount;
    private int mineHitCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureManagerOnSceneLoad()
    {
        GetOrCreate();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        trainingStartTime = Time.time;
        explosionClip = CreateExplosionClip();
        VRMineHUD.GetOrCreate();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.8f;
        }
    }

    public static MineDamageManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("MineDamageManager");
        return managerObject.AddComponent<MineDamageManager>();
    }

    public void ApplyMineHit(Vector3 minePosition)
    {
        if (trainingCleared)
            return;

        mineHitCount++;
        XRHapticFeedback.PulseControllers(1f, 0.35f);

        if (audioSource != null && explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip);
        }

        Transform playerRoot = FindPlayerRoot();
        if (playerRoot != null)
        {
            if (knockbackRoutine != null)
                StopCoroutine(knockbackRoutine);

            knockbackRoutine = StartCoroutine(KnockbackPlayer(playerRoot, minePosition));
        }

        centerMessage = "MINE HIT!";
        messageUntil = Time.time + messageDuration;
        VRMineHUD.GetOrCreate().ShowHit(messageDuration);
        Debug.Log($"Mine hit. Total hits: {mineHitCount}, score penalty: {mineHitCount * mineHitPenalty}");
    }

    public void CompleteTraining()
    {
        if (trainingCleared)
            return;

        trainingCleared = true;
        centerMessage = "";
        messageUntil = 0f;
        float elapsed = Mathf.Max(0f, Time.time - trainingStartTime);
        int elapsedPenaltySteps = elapsed <= fullScoreTime
            ? 0
            : Mathf.CeilToInt((elapsed - fullScoreTime) / Mathf.Max(1f, penaltyInterval));
        int timeScore = 100 - elapsedPenaltySteps * penaltyPerInterval;
        int finalScore = Mathf.Clamp(
            timeScore + defusedMineCount * defusedMineBonus - mineHitCount * mineHitPenalty,
            0,
            100);
        TrainingScoreManager.GetOrCreate().SetMineScore(finalScore);
        TrainingFlowManager.Instance?.CompleteModule("MineMap", true);
        Debug.Log($"Mine training clear. Time: {elapsed:0.0}s, Defused: {defusedMineCount}, Hits: {mineHitCount}, Score: {finalScore}");
    }

    public void NotifyMineDefused()
    {
        if (!trainingCleared)
            defusedMineCount++;
    }

    private Transform FindPlayerRoot()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            return playerObject.transform;

        if (Camera.main != null)
            return Camera.main.transform.root;

        return null;
    }

    private IEnumerator KnockbackPlayer(Transform playerRoot, Vector3 minePosition)
    {
        Vector3 start = playerRoot.position;
        Vector3 direction = playerRoot.position - minePosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = -playerRoot.forward;

        direction.Normalize();
        Vector3 end = start + direction * knockbackDistance + Vector3.up * verticalKick;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / knockbackDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            playerRoot.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        playerRoot.position = end;
        knockbackRoutine = null;
    }

    private AudioClip CreateExplosionClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.35f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float fade = 1f - (i / (float)sampleCount);
            float lowBoom = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.75f;
            float noise = Random.Range(-1f, 1f) * 0.35f;
            samples[i] = (lowBoom + noise) * fade;
        }

        AudioClip clip = AudioClip.Create("MineExplosionTest", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnGUI()
    {
        if (Time.time > messageUntil)
            return;

        GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow }
        };

        GUI.Label(new Rect(0f, Screen.height * 0.35f, Screen.width, 80f), centerMessage, messageStyle);
    }
}
