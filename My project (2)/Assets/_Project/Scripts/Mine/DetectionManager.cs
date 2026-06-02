using System.Collections.Generic;
using UnityEngine;

public class DetectionManager : MonoBehaviour
{
    public static DetectionManager Instance { get; private set; }

    [Tooltip("Maximum detection range in meters")]
    public float detectionRange = 3f;

    [Tooltip("Signal strength required to reveal a hidden mine.")]
    [Range(0f, 1f)]
    public float revealSignalThreshold = 0.65f;

    [Tooltip("Seconds to keep the detection message visible.")]
    public float messageDuration = 1.5f;

    [Tooltip("Optional UI to update (Slider/Text)")]
    public DetectionUI detectionUI;

    [Tooltip("Optional audio source for beeps")]
    public AudioSource audioSource;

    private readonly List<Mine> mines = new List<Mine>();
    private AudioClip beepClip;
    private float nextBeepTime;
    private float messageUntil;

    private void Awake()
    {
        Instance = this;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        beepClip = CreateBeepClip();
    }

    private void Start()
    {
        RefreshMines();
    }

    public void RefreshMines()
    {
        mines.Clear();
        Mine[] mineObjects = FindObjectsOfType<Mine>();
        foreach (Mine mine in mineObjects)
        {
            mines.Add(mine);
        }
    }

    public void UnregisterMine(Mine mine)
    {
        mines.Remove(mine);
    }

    public void UpdateDetection(Vector3 detectorPosition)
    {
        float maxSignal = 0f;
        Mine nearestMine = null;
        HashSet<Mine> minesInRange = new HashSet<Mine>();

        for (int i = mines.Count - 1; i >= 0; i--)
        {
            Mine mine = mines[i];
            if (mine == null)
            {
                mines.RemoveAt(i);
                continue;
            }

            if (mine.IsNeutralized())
            {
                mines.RemoveAt(i);
                continue;
            }

            if (!mine.CanEmitDetectionSignal())
                continue;

            float d = Vector3.Distance(detectorPosition, mine.transform.position);
            if (d > detectionRange) continue;
            minesInRange.Add(mine);
            float s = Mathf.Clamp01(1f - (d / detectionRange));
            if (s > maxSignal)
            {
                maxSignal = s;
                nearestMine = mine;
            }
        }

        if (nearestMine != null && nearestMine.CanBeDetected() && maxSignal >= revealSignalThreshold)
        {
            nearestMine.Reveal();
            messageUntil = Time.time + messageDuration;
            VRMineHUD.GetOrCreate().ShowDetection(messageDuration);
        }

        HideRevealedMinesOutsideRange(minesInRange);

        detectionUI?.UpdateDetectionUI(maxSignal);
        UpdateAudio(maxSignal);
    }

    public void StopDetection()
    {
        HideRevealedMinesOutsideRange(new HashSet<Mine>());
        detectionUI?.UpdateDetectionUI(0f);
    }

    private void HideRevealedMinesOutsideRange(HashSet<Mine> minesInRange)
    {
        foreach (Mine mine in mines)
        {
            if (mine == null || mine.IsNeutralized() || mine.IsDefusalInProgress())
                continue;

            if (!minesInRange.Contains(mine))
            {
                mine.HideIfNotActive();
            }
        }
    }

    private void OnGUI()
    {
        if (Time.time > messageUntil)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.yellow;

        Rect rect = new Rect(0f, Screen.height * 0.28f, Screen.width, 90f);
        GUI.Label(rect, "지뢰 탐지", style);
    }

    private void UpdateAudio(float signal)
    {
        if (audioSource == null) return;
        if (signal <= 0.01f)
        {
            return;
        }

        float interval = Mathf.Lerp(0.65f, 0.12f, signal);
        if (Time.time < nextBeepTime)
            return;

        audioSource.volume = Mathf.Lerp(0.15f, 0.8f, signal);
        audioSource.pitch = Mathf.Lerp(0.8f, 1.8f, signal);
        audioSource.PlayOneShot(beepClip);
        XRHapticFeedback.PulseControllers(Mathf.Lerp(0.08f, 0.32f, signal), 0.05f);
        nextBeepTime = Time.time + interval;
    }

    private AudioClip CreateBeepClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.08f;
        const float frequency = 880f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float fade = 1f - (i / (float)sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.5f;
        }

        AudioClip clip = AudioClip.Create("MineDetectorBeep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
