using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleOutcomeManager : MonoBehaviour
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

    [Header("UI References")]
    public GameObject mainMenuButton;
    public GameObject restartButton;
    public GameObject nextLevelButton;
    public TMP_Text winText;   
    public TMP_Text loseText;  

    private bool gameHasEnded = false;

    private List<GameObject> activePlayers = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> pendingDestroyObjects = new List<GameObject>();

    void Start()
    {
        if (mainMenuButton) mainMenuButton.SetActive(false);
        if (restartButton) restartButton.SetActive(false);
        if (nextLevelButton) nextLevelButton.SetActive(false);
        if (winText) winText.gameObject.SetActive(false);
        if (loseText) loseText.gameObject.SetActive(false);

        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        activePlayers.AddRange(players);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        activeEnemies.AddRange(enemies);
    }

    void Update()
    {
        if (gameHasEnded) return;

        activePlayers.RemoveAll(p => p == null);
        activeEnemies.RemoveAll(e => e == null);

        // Win: all enemies defeated AND only 1 player remains
        if (activeEnemies.Count == 0 && activePlayers.Count == 1)
        {
            StartCoroutine(TriggerWinCoroutine(activePlayers[0]));
        }
        // Lose: no players left
        else if (activePlayers.Count == 0)
        {
            TriggerLose();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameHasEnded) return;

        if (other.CompareTag(playerTag) && activePlayers.Contains(other.gameObject))
        {
            activePlayers.Remove(other.gameObject);
            pendingDestroyObjects.Add(other.gameObject);
            StartCoroutine(DelayedDestroy(other.gameObject));
        }
        else if (other.CompareTag(enemyTag) && activeEnemies.Contains(other.gameObject))
        {
            activeEnemies.Remove(other.gameObject);
            pendingDestroyObjects.Add(other.gameObject);
            StartCoroutine(DelayedDestroy(other.gameObject));
        }
    }

    private IEnumerator TriggerWinCoroutine(GameObject winner)
    {
        gameHasEnded = true;
        yield return new WaitForSeconds(winDelay);

        if (winText) winText.text = $"{winner.name} Wins!";
        if (winText) winText.gameObject.SetActive(true);

        if (mainMenuButton) mainMenuButton.SetActive(true);
        if (nextLevelButton) nextLevelButton.SetActive(true);
    }

    private void TriggerLose()
    {
        gameResult = GameResult.Lose;
        Debug.Log("You Lose!");

        if (loseText) loseText.gameObject.SetActive(true);
        if (mainMenuButton) mainMenuButton.SetActive(true);
        if (restartButton) restartButton.SetActive(true);
    }

    private IEnumerator DelayedDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(objectDestructionDelay);
        if (obj) Destroy(obj);
        pendingDestroyObjects.Remove(obj);
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
