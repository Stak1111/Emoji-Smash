using UnityEngine;

public class SceneSlowMotion : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float slowFactor = 0.5f; // Adjust in Inspector

    private float originalTimeScale;

    void Awake()
    {
        // Save original
        originalTimeScale = Time.timeScale;

        ApplySlowMotion();
    }

    void ApplySlowMotion()
    {
        // Slow timers, animations, coroutines, etc.
        Time.timeScale = slowFactor;

        // 🚫 Don't touch fixedDeltaTime — leave physics running at normal rate
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplySlowMotion();
        }
    }

    void OnDestroy()
    {
        // Restore normal
        Time.timeScale = originalTimeScale;
    }

    void OnDisable() 
{
    Time.timeScale = 1f; // reset normal speed
}
}
