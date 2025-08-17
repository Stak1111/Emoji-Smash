using UnityEngine;
using TMPro;

public class LevelTimerUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;

    private float startTime;
    private bool isRunning;

    void Awake()
    {
        if (timerText != null)
            timerText.gameObject.SetActive(false); // hide timer until win
    }

    void Start()
    {
        // Start the timer for this scene
        StartTimer();
        Debug.Log($"LevelTimerUI started in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
    }

    void Update()
    {
        if (!isRunning) return;

        float elapsed = Time.time - startTime;

        // Optional: debug log to see timer running
        Debug.Log($"LevelTimerUI running: elapsed = {elapsed:0.00} sec");
    }

    /// <summary>
    /// Starts the timer for this level
    /// </summary>
    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
    }

    /// <summary>
    /// Stops the timer. Only adds to WorldTimer if the level is won.
    /// Reveals timer UI on win.
    /// </summary>
    /// <param name="won">True if player won this level</param>
    public void StopTimer(bool won)
    {
        if (!isRunning) return;

        float elapsed = Time.time - startTime;
        isRunning = false;

        if (won)
        {
            // Add current level time to WorldTimer
            WorldTimer.AddLevelTime(elapsed);
            Debug.Log($"LevelTimerUI stopped. Added {elapsed:0.00} sec to WorldTimer. Total = {WorldTimer.GetTotalTime():0.00} sec");

            // Reveal timer text showing total accumulated time
            if (timerText != null)
            {
                timerText.text = $"Level Time: {elapsed:0.00} sec\nTotal Time: {WorldTimer.GetTotalTime():0.00} sec";
                timerText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log($"LevelTimerUI stopped. Level lost. Time discarded: {elapsed:0.00} sec");
        }
    }

    /// <summary>
    /// Resets the timer for the next level. Does not affect WorldTimer.
    /// </summary>
    public void ResetTimer()
    {
        isRunning = false;

        if (timerText != null)
            timerText.text = "0.00 sec";
        
        Debug.Log("LevelTimerUI reset.");
    }

    /// <summary>
    /// Call at the final level to show the cumulative total time
    /// </summary>
    public void ShowTotalTime()
    {
        if (timerText != null)
        {
            timerText.text = $"Total Time: {WorldTimer.GetTotalTime():0.00} sec";
            timerText.gameObject.SetActive(true);
        }

        Debug.Log($"Total time displayed: {WorldTimer.GetTotalTime():0.00} sec");
    }
}
