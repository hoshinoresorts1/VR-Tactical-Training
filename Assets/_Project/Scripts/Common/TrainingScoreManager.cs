using UnityEngine;

public class TrainingScoreManager : MonoBehaviour
{
    public const int MaxModuleScore = 100;
    public const int MaxTotalScore = MaxModuleScore * 4;

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

    public static TrainingScoreManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("TrainingScoreManager");
        return managerObject.AddComponent<TrainingScoreManager>();
    }

    public void AddGearScore(int amount)
    {
        SetGearScore(gearScore + amount);
    }

    public void AddShootingScore(int amount)
    {
        SetShootingScore(shootingScore + amount);
    }

    public void AddCQBScore(int amount)
    {
        SetCQBScore(cqbScore + amount);
    }

    public void AddMineScore(int amount)
    {
        SetMineScore(mineScore + amount);
    }

    public void SetGearScore(int value) => gearScore = ClampModuleScore(value);
    public void SetShootingScore(int value) => shootingScore = ClampModuleScore(value);
    public void SetCQBScore(int value) => cqbScore = ClampModuleScore(value);
    public void SetMineScore(int value) => mineScore = ClampModuleScore(value);

    public void ResetScores()
    {
        gearScore = 0;
        shootingScore = 0;
        cqbScore = 0;
        mineScore = 0;
    }

    public int GetTotalScore()
    {
        return gearScore + shootingScore + cqbScore + mineScore;
    }

    private int ClampModuleScore(int value)
    {
        return Mathf.Clamp(value, 0, MaxModuleScore);
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
