using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseZone : MonoBehaviour
{
    [Header("Tags")]
    public string playerTag = "Player";
    public string enemyTag = "Enemy";

    [Header("Delays")]
    public float playerLoseDelay = 2f;           // Time after player is destroyed before showing UI
    public float objectDestructionDelay = 1f;    // Time before destroying the player/enemy GameObject

    [Header("UI Elements")]
    public GameObject loseUIPanel;
    public Button restartButton;
    public Button mainMenuButton;

    private bool playerLostTriggered = false;

    private void Start()
    {
        // Hide UI on start
        if (loseUIPanel != null)
            loseUIPanel.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);

        // Assign button actions
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameStateManager.hasGameEnded)
            return;

        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered LoseZone.");
            StartCoroutine(DelayedDestroyAndTriggerLose(other.gameObject));
        }
        else if (other.CompareTag(enemyTag))
        {
            Debug.Log("Enemy entered LoseZone.");
            StartCoroutine(DelayedDestroy(other.gameObject));
        }
    }

    private IEnumerator DelayedDestroy(GameObject target)
    {
        yield return new WaitForSeconds(objectDestructionDelay);
        if (target != null)
            Destroy(target);
    }

    private IEnumerator DelayedDestroyAndTriggerLose(GameObject playerObj)
    {
        yield return new WaitForSeconds(objectDestructionDelay);

        if (playerObj != null)
            Destroy(playerObj);

        if (!playerLostTriggered && !GameStateManager.hasGameEnded)
        {
            playerLostTriggered = true;
            GameStateManager.hasGameEnded = true;

            yield return new WaitForSeconds(playerLoseDelay);
            Debug.Log("Game Over!");
            ShowLoseUI();
        }
    }

    private void ShowLoseUI()
    {
        if (loseUIPanel != null)
            loseUIPanel.SetActive(true);

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace with your actual main menu scene name
    }
}
