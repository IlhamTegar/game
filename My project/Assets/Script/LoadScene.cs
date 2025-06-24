using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadScene : MonoBehaviour
{
    
    public TMP_Text countdownText; // Drag CountdownText dari GameOverPanel
    public string mainMenuSceneName = "MainMenu"; // Ganti sesuai nama scene Main Menu
    private float countdownTime = 10f;
    private bool isCountingDown = false;
    public GameObject gameOverPanel;

    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void paused()
    {
        Time.timeScale = 0;
    }

    public void resume()
    {
        Time.timeScale = 1;
    }

    public void ShowGameOverScreen()
    {
        paused(); // Time.timeScale = 0
        gameOverPanel.SetActive(true); // Munculkan panel
        if (!isCountingDown)
        {
            StartCoroutine(GameOverCountdown());
            isCountingDown = true;
        }
    }

    private IEnumerator GameOverCountdown()
    {
        float timer = countdownTime;

        while (timer > 0)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timer).ToString();

            yield return new WaitForSecondsRealtime(1f); // pakai WaitForSecondsRealtime karena Time.timeScale = 0
            timer -= 1f;
        }

        resume(); // Unpause sebelum pindah scene
        ChangeScene(mainMenuSceneName);
    }
}
