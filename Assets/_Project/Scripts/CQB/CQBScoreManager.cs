using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CQBScoreManager : MonoBehaviour
{
    public int score = 0;
    public int enemyLeft = 10;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI enemyText;
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI timeText;

    [Header("Result Popup")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public string restartSceneName = "GearScene";

    [Header("Scene Portals")]
    public bool ensureScenePortals = true;
    public string[] portalSceneNames = new string[] { "GearScene", "MineMap", "ShootingScene" };

    private float playTime = 0f;
    private bool missionComplete = false;
    public bool IsMissionComplete => missionComplete;

    void Start()
    {
        Time.timeScale = 1f;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        EnsureRestartButton();
        UpdateUI();
        UpdateTimeUI();

        if (missionText != null)
            missionText.text = "Mission : Clear Area";

        EnsureScenePortals();
    }

    void Update()
    {
        if (missionComplete)
            return;

        playTime += Time.deltaTime;
        UpdateTimeUI();
    }

    public void HitEnemy()
    {
        if (missionComplete)
            return;

        score = Mathf.Clamp(score + 10, 0, 100);
        enemyLeft--;

        if (enemyLeft < 0)
            enemyLeft = 0;

        UpdateUI();

        if (enemyLeft <= 0)
            MissionComplete();
    }

    public void HitCivilian()
    {
        if (missionComplete)
            return;

        score = Mathf.Clamp(score - 20, 0, 100);
        UpdateUI();
    }

    public void AddPenalty(int penalty)
    {
        if (missionComplete)
            return;

        score = Mathf.Clamp(score - penalty, 0, 100);
        UpdateUI();
    }

    void MissionComplete()
    {
        missionComplete = true;
        score = Mathf.Clamp(score, 0, 100);
        TrainingScoreManager.GetOrCreate().SetCQBScore(score);
        TrainingFlowManager.Instance?.CompleteModule("CQBScene", true);

        if (scoreText != null)
            scoreText.gameObject.SetActive(false);

        if (enemyText != null)
            enemyText.gameObject.SetActive(false);

        if (missionText != null)
            missionText.gameObject.SetActive(false);

        if (timeText != null)
            timeText.gameObject.SetActive(false);

        ShowResultPopup();

        // NOTE: do NOT freeze time (Time.timeScale = 0) — it stops XR locomotion
        // so the player can't walk to a SceneDoor to leave. Keep the world running
        // and show the result popup instead.
        Time.timeScale = 1f;
        Debug.Log("MISSION COMPLETE");
    }

    void ShowResultPopup()
    {
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);
        TrainingScoreManager scores = TrainingScoreManager.GetOrCreate();

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text =
                "TRAINING COMPLETE\n\n" +
                $"Gear Score : {scores.GearScore} / 100\n" +
                $"Mine Score : {scores.MineScore} / 100\n" +
                $"Shooting Score : {scores.ShootingScore} / 100\n" +
                $"CQB Score : {scores.CQBScore} / 100\n\n" +
                $"Total Score : {scores.GetTotalScore()} / {TrainingScoreManager.MaxTotalScore}\n" +
                $"CQB Clear Time : {minutes:00}:{seconds:00}";
        }
    }

    private void EnsureRestartButton()
    {
        if (resultPanel == null)
            return;

        Transform existingButton = resultPanel.transform.Find("RestartTrainingButton");
        if (existingButton != null)
        {
            RectTransform existingRect = existingButton.GetComponent<RectTransform>();
            if (existingRect != null)
                existingRect.anchoredPosition = new Vector2(0f, -310f);
            return;
        }

        GameObject buttonObject = new GameObject("RestartTrainingButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(resultPanel.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -310f);
        rect.sizeDelta = new Vector2(300f, 70f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.45f, 0.8f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(RestartTraining);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "Restart Training";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 34f;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    public void RestartTraining()
    {
        TrainingScoreManager.GetOrCreate().ResetScores();
        TrainingFlowManager.Instance?.ResetCourse();

        if (Application.CanStreamedLevelBeLoaded(restartSceneName))
            SceneManager.LoadScene(restartSceneName);
        else
            Debug.LogWarning("Restart scene is not in Build Settings: " + restartSceneName, this);
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;

        if (enemyText != null)
            enemyText.text = "Enemy Left : " + enemyLeft;
    }

    void UpdateTimeUI()
    {
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);

        if (timeText != null)
            timeText.text = $"Time : {minutes:00}:{seconds:00}";
    }

    private void EnsureScenePortals()
    {
        if (!ensureScenePortals || portalSceneNames == null || portalSceneNames.Length == 0)
            return;

        if (FindObjectsOfType<SceneDoor>().Length > 0)
            return;

        Camera cam = Camera.main;
        Vector3 basePosition = cam != null ? cam.transform.position : transform.position;
        basePosition.y = 0f;

        Vector3 forward = cam != null ? cam.transform.forward : transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 center = basePosition + forward * 3f;

        for (int i = 0; i < portalSceneNames.Length; i++)
        {
            string sceneName = portalSceneNames[i];
            float offset = (i - (portalSceneNames.Length - 1) * 0.5f) * 1.35f;
            CreatePortal(sceneName, center + right * offset, forward);
        }
    }

    private void CreatePortal(string sceneName, Vector3 position, Vector3 forward)
    {
        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "Door_" + sceneName;
        portal.transform.position = position + Vector3.up;
        portal.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        portal.transform.localScale = new Vector3(0.8f, 2f, 0.18f);

        SceneDoor door = portal.AddComponent<SceneDoor>();
        door.targetSceneName = sceneName;
        door.triggerRadius = 1f;
        door.dwell = 0.15f;

        Renderer renderer = portal.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader != null) renderer.material = new Material(shader) { color = new Color(0.05f, 0.55f, 0.85f, 1f) };
        }

        GameObject label = new GameObject("Label");
        label.transform.SetParent(portal.transform, false);
        label.transform.localPosition = new Vector3(0f, 0.7f, -0.6f);
        label.transform.localRotation = Quaternion.identity;
        TextMesh text = label.AddComponent<TextMesh>();
        text.text = sceneName;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.18f;
        text.color = Color.white;
    }
}
