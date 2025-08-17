using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOutcomeManager : MonoBehaviour
{
    public enum GameResult { None, Win, Lose }
    public GameResult gameResult = GameResult.None;
    

    [Header("Tags")]
    public string enemyTag = "Enemy";
    public string playerTag = "Player";

    [Header("Delays")]
    public float winDelay = 1f;
    public float playerLoseDelay = 1f;
    public float objectDestructionDelay = 0.5f;

    [Header("References")]
    public LevelTimerUI levelTimerUI; 

    [Header("UI References")]
    public GameObject mainMenuButton;
    public GameObject restartButton;
    public GameObject nextLevelButton;
    public TMP_Text winText;   
    public TMP_Text loseText;  

    private bool gameHasEnded = false;

    // Track all enemies in the scene
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> pendingDestroyEnemies = new List<GameObject>();

    void Start()
    {
        if (mainMenuButton) mainMenuButton.SetActive(false);
        if (restartButton) restartButton.SetActive(false);
        if (nextLevelButton) nextLevelButton.SetActive(false);
        if (winText) winText.gameObject.SetActive(false);
        if (loseText) loseText.gameObject.SetActive(false);

        // Initialize enemy list
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        activeEnemies.AddRange(enemies);
    }

    void Update()
    {
        if (gameHasEnded) return;

        // Remove any enemies that were destroyed visually but not yet removed from lists
        activeEnemies.RemoveAll(e => e == null);

        // Check for auto-win
        if (activeEnemies.Count == 0)
        {
            StartCoroutine(TriggerWinCoroutine());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameHasEnded) return;

        if (other.CompareTag(playerTag))
        {
            StartCoroutine(DelayedDestroyAndTriggerLose(other.gameObject));
        }
        else if (other.CompareTag(enemyTag))
        {
            // Enemy hit something — remove from active list immediately
            if (activeEnemies.Contains(other.gameObject))
            {
                activeEnemies.Remove(other.gameObject);
                pendingDestroyEnemies.Add(other.gameObject);
                StartCoroutine(DelayedDestroy(other.gameObject));
            }
        }
    }

    // --- WIN ---
    private IEnumerator TriggerWinCoroutine()
    {
        gameHasEnded = true;
        yield return new WaitForSeconds(winDelay);
        TriggerWin();
    }

    private void TriggerWin()
    {
        gameResult = GameResult.Win;
        Debug.Log("You Win!");
        if (levelTimerUI) levelTimerUI.StopTimer(true);

        if (mainMenuButton) mainMenuButton.SetActive(true);
        if (nextLevelButton) nextLevelButton.SetActive(true);
        if (winText) winText.gameObject.SetActive(true);
    }

    // --- LOSE ---
    private IEnumerator DelayedDestroyAndTriggerLose(GameObject playerObj)
    {
        gameHasEnded = true;

        yield return new WaitForSeconds(objectDestructionDelay);
        if (playerObj) Destroy(playerObj);

        yield return new WaitForSeconds(playerLoseDelay);
        TriggerLose();
    }

    private void TriggerLose()
    {
        gameResult = GameResult.Lose;
        Debug.Log("You Lose!");
        if (levelTimerUI) levelTimerUI.StopTimer(false);

        if (mainMenuButton) mainMenuButton.SetActive(true);
        if (restartButton) restartButton.SetActive(true);
        if (loseText) loseText.gameObject.SetActive(true);
    }

    private IEnumerator DelayedDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(objectDestructionDelay);
        if (obj) Destroy(obj);
        pendingDestroyEnemies.Remove(obj);
    }

    // --- Public UI buttons ---
    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
    public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.Log("No next level found in build settings!");
    }
}
