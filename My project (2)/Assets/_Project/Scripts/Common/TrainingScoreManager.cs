using UnityEngine;

public class TrainingScoreManager : MonoBehaviour
{
    public static TrainingScoreManager Instance { get; private set; }

    public int gearScore;
    public int shootingScore;
    public int cqbScore;
    public int mineScore;

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

    public void AddGearScore(int amount) => gearScore += amount;
    public void AddShootingScore(int amount) => shootingScore += amount;
    public void AddCQBScore(int amount) => cqbScore += amount;
    public void AddMineScore(int amount) => mineScore += amount;

    public int GetTotalScore()
    {
        return gearScore + shootingScore + cqbScore + mineScore;
    }
}
