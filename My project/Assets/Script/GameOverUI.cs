using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public LoadScene loadScene;
    public Button returnButton;

    void Start()
    {
        gameOverPanel.SetActive(false);
        returnButton.onClick.AddListener(() => loadScene.ShowGameOverScreen());
    }

    public void ActivateGameOver()
    {
        gameOverPanel.SetActive(true);
        loadScene.ShowGameOverScreen();
    }
}
