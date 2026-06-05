using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MineDamageManager : MonoBehaviour
{
    public static MineDamageManager Instance { get; private set; }

    [Header("Lives")]
    public int maxLives = 3;
    public int scorePenalty = -10;

    [Header("Hit Feedback")]
    public float knockbackDistance = 0.35f;
    public float verticalKick = 0f;
    public float knockbackDuration = 0.25f;
    public float messageDuration = 1.2f;
    public AudioSource audioSource;

    private int lives;
    private bool gameOver;
    private bool trainingCleared;
    private string centerMessage = "";
    private float messageUntil;
    private AudioClip explosionClip;
    private Coroutine knockbackRoutine;
    private readonly List<Behaviour> disabledMovementBehaviours = new List<Behaviour>();
    private readonly List<Rigidbody> lockedRigidbodies = new List<Rigidbody>();

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
        lives = Mathf.Max(1, maxLives);
        explosionClip = CreateExplosionClip();
        VRMineHUD.GetOrCreate().SetLives(lives);

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
        if (gameOver || trainingCleared)
            return;

        lives = Mathf.Max(0, lives - 1);
        TrainingScoreManager.Instance?.AddMineScore(scorePenalty);
        VRMineHUD.GetOrCreate().SetLives(lives);
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

        gameOver = lives <= 0;
        centerMessage = gameOver ? "GAME OVER" : "MINE HIT!";
        messageUntil = Time.time + messageDuration;

        if (gameOver)
        {
            LockPlayerMovement();
            VRMineHUD.GetOrCreate().ShowGameOver();
        }
        else
        {
            VRMineHUD.GetOrCreate().ShowHit(messageDuration);
        }

        Debug.Log(gameOver ? "Mine lives depleted. Game over." : "Mine hit. Lives left: " + lives);
    }

    public void CompleteTraining()
    {
        if (gameOver || trainingCleared)
            return;

        trainingCleared = true;
        centerMessage = "CLEAR";
        messageUntil = float.PositiveInfinity;
        TrainingScoreManager.Instance?.AddMineScore(100);
        LockPlayerMovement();
        VRMineHUD.GetOrCreate().ShowClear();
        Debug.Log("Mine training clear.");
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

    private void LockPlayerMovement()
    {
        Transform playerRoot = FindPlayerRoot();
        if (playerRoot == null)
            return;

        disabledMovementBehaviours.Clear();
        lockedRigidbodies.Clear();

        Behaviour[] behaviours = playerRoot.GetComponentsInChildren<Behaviour>(true);
        foreach (Behaviour behaviour in behaviours)
        {
            if (behaviour == null || !behaviour.enabled)
                continue;

            if (!LooksLikeMovementBehaviour(behaviour))
                continue;

            behaviour.enabled = false;
            disabledMovementBehaviours.Add(behaviour);
        }

        CharacterController controller = playerRoot.GetComponentInChildren<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        Rigidbody[] rigidbodies = playerRoot.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody body in rigidbodies)
        {
            if (body == null)
                continue;

            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            lockedRigidbodies.Add(body);
        }
    }

    private bool LooksLikeMovementBehaviour(Behaviour behaviour)
    {
        string typeName = behaviour.GetType().Name;
        string fullName = behaviour.GetType().FullName ?? typeName;

        return typeName.Contains("MoveProvider")
            || typeName.Contains("TurnProvider")
            || typeName.Contains("Locomotion")
            || typeName.Contains("TeleportationProvider")
            || fullName.Contains("Locomotion.Movement")
            || fullName.Contains("Locomotion.Turning");
    }

    private void Retry()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
        else
        {
            SceneManager.LoadScene(activeScene.name);
        }
    }

    private void OnGUI()
    {
        GUIStyle livesStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUI.Label(new Rect(24f, 24f, 220f, 48f), "LIVES: " + lives, livesStyle);

        if (Time.time > messageUntil && !gameOver && !trainingCleared)
            return;

        GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = gameOver || trainingCleared ? 48 : 36,
            fontStyle = FontStyle.Bold,
            normal = { textColor = gameOver ? Color.red : trainingCleared ? Color.green : Color.yellow }
        };

        GUI.Label(new Rect(0f, Screen.height * 0.35f, Screen.width, 80f), centerMessage, messageStyle);

        if (!gameOver)
            return;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold
        };

        Rect retryRect = new Rect((Screen.width - 220f) * 0.5f, Screen.height * 0.48f, 220f, 64f);
        if (GUI.Button(retryRect, "RETRY", buttonStyle))
            Retry();

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            Retry();
        }
#else
        if (Input.GetKeyDown(KeyCode.R))
        {
            Retry();
        }
#endif
    }
}
