using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneTitleSplash : MonoBehaviour
{
    [Header("Sprite to display as title page")]
    public GameObject titleSprite; // Assign a UI Image or SpriteRenderer GameObject

    [Header("Timing")]
    public float delayBeforeShow = 1f;
    public float showDuration = 3f;

    // Tracks if splash has already been shown per scene
    private static readonly HashSet<string> scenesShown = new HashSet<string>();

    void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (scenesShown.Contains(sceneName))
        {
            // Already shown in this scene, skip entirely
            if (titleSprite != null)
                Destroy(titleSprite);
            Destroy(this); // Remove script so it doesn't run
            return;
        }

        // First time showing the splash in this scene
        scenesShown.Add(sceneName);
        Time.timeScale = 0f;

        if (titleSprite != null)
            titleSprite.SetActive(false); // Hide initially
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!scenesShown.Contains(sceneName)) return; // Skip if somehow already marked

        StartCoroutine(SplashRoutine());
    }

    IEnumerator SplashRoutine()
    {
        yield return new WaitForSecondsRealtime(delayBeforeShow);

        if (titleSprite != null)
            titleSprite.SetActive(true);

        yield return new WaitForSecondsRealtime(showDuration);

        if (titleSprite != null)
            Destroy(titleSprite);

        Time.timeScale = 1f;
    }
}
