using UnityEngine;
using UnityEngine.UI;

public class PlayerSetupUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel; // The parent panel for all buttons
    public Button player1Button;
    public Button player2Button;
    public Button player3Button;
    public Button player4Button;

    [Header("Player References")]
    public GameObject[] players; // Assign all 4 player prefabs in the inspector in correct order

    private int selectedPlayerCount = 1;

    void Start()
    {
        // Pause the scene
        Time.timeScale = 0f;

        // Show panel
        panel.SetActive(true);

        // Bind buttons
        player1Button.onClick.AddListener(() => ChoosePlayers(1));
        player2Button.onClick.AddListener(() => ChoosePlayers(2));
        player3Button.onClick.AddListener(() => ChoosePlayers(3));
        player4Button.onClick.AddListener(() => ChoosePlayers(4));

        // Disable all players at start
        DisableAllPlayers();
    }

    private void DisableAllPlayers()
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                var controller = player.GetComponent<MonoBehaviour>();
                if (controller != null)
                    controller.enabled = false;
            }
        }
    }

    private void ChoosePlayers(int count)
    {
        selectedPlayerCount = count;

        // Hide panel completely
        panel.SetActive(false);

        // Enable selected players and destroy extras
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;

            var controller = players[i].GetComponent<MonoBehaviour>();
            if (i < count)
            {
                if (controller != null)
                    controller.enabled = true;
            }
            else
            {
                Destroy(players[i]);
            }
        }

        // Unpause the scene
        Time.timeScale = 1f;
    }
}
