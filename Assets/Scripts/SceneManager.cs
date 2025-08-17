using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMenuManager : MonoBehaviour
{
    // === MAIN MENU ===

    // Called by the "Story Mode" button
    public void LoadStoryMode()
    {
        SceneManager.LoadScene("Level 1"); // Make sure "Level1" is added to Build Settings
    }

    // Called by the "Battle Mode" button
    public void LoadBattleMode()
    {
        SceneManager.LoadScene("Battlemode"); // Ensure "Battlemode" scene is added to Build Settings
    }

    // Called by the "Exit" button
    public void ExitGame()
    {
        Debug.Log("Exiting game...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
        #else
        Application.Quit(); // Quit application in build
        #endif
    }

    // === IN-GAME OR WIN/LOSE SCREENS ===

    // Goes to the next scene in Build Settings
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Optional safety check
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            Debug.LogWarning("No next level in build settings.");
    }

    // Reloads the current scene
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Returns to Main Menu
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace with your actual Main Menu scene name
    }
}
