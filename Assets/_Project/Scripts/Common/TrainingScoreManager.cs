using UnityEngine;

public class TrainingScoreManager : MonoBehaviour
{
    public static TrainingScoreManager Instance { get; private set; }

    [SerializeField] private int gearScore;
    [SerializeField] private int shootingScore;
    [SerializeField] private int cqbScore;
    [SerializeField] private int mineScore;

    public int GearScore => gearScore;
    public int ShootingScore => shootingScore;
    public int CQBScore => cqbScore;
    public int MineScore => mineScore;

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

    public void AddGearScore(int amount)
    {
        gearScore += amount;
    }

    public void AddShootingScore(int amount)
    {
        shootingScore += amount;
    }

    public void AddCQBScore(int amount)
    {
        cqbScore += amount;
    }

    public void AddMineScore(int amount)
    {
        mineScore += amount;
    }

    public int GetTotalScore()
    {
        return gearScore + shootingScore + cqbScore + mineScore;
    }

    public string GetResultText()
    {
        int totalScore = GetTotalScore();

        if (totalScore >= 90)
        {
            return "조기퇴근";
        }

        if (totalScore >= 70)
        {
            return "훈련 통과";
        }

        if (totalScore >= 50)
        {
            return "재훈련 권고";
        }

        return "얼차려";
    }
}
