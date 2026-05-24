using System.Collections.Generic;
using UnityEngine;

public class DetectionManager : MonoBehaviour
{
    public static DetectionManager Instance { get; private set; }

    [Tooltip("Maximum detection range in meters")]
    public float detectionRange = 10f;

    [Tooltip("Optional UI to update (Slider/Text)")]
    public DetectionUI detectionUI;

    [Tooltip("Optional audio source for beeps")]
    public AudioSource audioSource;

    private List<Transform> mines = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshMines();
    }

    public void RefreshMines()
    {
        mines.Clear();
        var mineObjects = GameObject.FindGameObjectsWithTag("Mine");
        foreach (var go in mineObjects)
            mines.Add(go.transform);
    }

    public void UpdateDetection(Vector3 detectorPosition)
    {
        float maxSignal = 0f;
        foreach (var m in mines)
        {
            if (m == null) continue;
            float d = Vector3.Distance(detectorPosition, m.position);
            if (d > detectionRange) continue;
            float s = Mathf.Clamp01(1f - (d / detectionRange));
            if (s > maxSignal) maxSignal = s;
        }

        detectionUI?.UpdateDetectionUI(maxSignal);
        UpdateAudio(maxSignal);
    }

    private void UpdateAudio(float signal)
    {
        if (audioSource == null) return;
        if (signal <= 0.01f)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            return;
        }

        audioSource.volume = Mathf.Lerp(0.1f, 1f, signal);
        audioSource.pitch = 1f + signal * 1.2f;
        if (!audioSource.isPlaying) audioSource.Play();
    }
}
