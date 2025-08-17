using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinCondition : MonoBehaviour
{
    [Header("Tags")]
    public string enemyTag = "Enemy"; // All enemies must be tagged properly

    [Header("Win Delay")]
    public float winDelay = 1f; // Delay before showing win UI after all enemies are destroyed

    [Header("UI Elements")]
    public GameObject winUIPanel;
    public Button nextLevelButton;
    public Button restartButton;
    public Button mainMenuButton;

    private bool winTriggered = false;

    private void Start()
    {
        // Hide UI on start
        if (winUIPanel != null)
            winUIPanel.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);

        // Assign button actions
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Update()
    {
        if (!winTriggered && !GameStateManager.hasGameEnded && GameObject.FindGameObjectsWithTag(enemyTag).Length == 0)
        {
            StartCoroutine(TriggerWin());
        }
    }

    private IEnumerator TriggerWin()
    {
        winTriggered = true;
        GameStateManager.hasGameEnded = true;

        yield return new WaitForSeconds(winDelay);
        Debug.Log("🎉 Player Wins!");
        ShowWinUI();
    }

    private void ShowWinUI()
    {
        if (winUIPanel != null)
            winUIPanel.SetActive(true);

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(true);

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace with actual main menu scene name
    }

    private void LoadNextLevel()
    {
        // This assumes scenes are in Build Settings in order
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            Debug.Log("✅ No more levels to load.");
    }
}
