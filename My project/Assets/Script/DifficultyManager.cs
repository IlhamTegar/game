// DifficultyManager.cs
using UnityEngine;

public enum DifficultyLevel { Bayi, Normal, HellNah }

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;
    public DifficultyLevel SelectedDifficulty = DifficultyLevel.Normal;

    public int deathCount = 0;
    public bool normalCompleted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(DifficultyLevel level)
    {
        SelectedDifficulty = level;
    }

    public bool IsBayiUnlocked() => deathCount >= 5;
    public bool IsHellNahUnlocked() => normalCompleted;
}
