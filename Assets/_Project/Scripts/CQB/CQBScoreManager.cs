using UnityEngine;
using TMPro;

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

    private float playTime = 0f;
    private bool missionComplete = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateUI();
        UpdateTimeUI();

        if (missionText != null)
            missionText.text = "Mission : Clear Area";
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

        score += 10;
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

        score -= 20;
        UpdateUI();
    }

    public void AddPenalty(int penalty)
    {
        if (missionComplete)
            return;

        score -= penalty;
        UpdateUI();
    }

    void MissionComplete()
    {
        missionComplete = true;

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

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text =
                "MISSION COMPLETE\n\n" +
                "Final Score : " + score + "\n" +
                $"Clear Time : {minutes:00}:{seconds:00}";
        }
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
}