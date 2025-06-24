using UnityEngine;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance; // Singleton akses global

    public int bossHealth = 100;
    public string nextSceneName;
    public GameObject toBeContinuedPanel;

    private bool isDefeated = false;

    private void Awake()
    {
        // Buat Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDefeated) return;

        bossHealth -= damage;
        if (bossHealth <= 0)
        {
            bossHealth = 0;
            isDefeated = true;
            HandleBossDefeated();
        }
    }

    public void HandleBossDefeated()
    {
        if (toBeContinuedPanel != null)
        {
            toBeContinuedPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
