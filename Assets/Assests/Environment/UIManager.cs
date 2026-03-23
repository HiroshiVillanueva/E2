using UnityEngine;
using TMPro; // Required to use TextMeshPro UI elements
using UnityEngine.SceneManagement;

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Acts as the central hub for the game's UI and mission logic. It tracks the 
 * player's kill count, updates on-screen text, flips a global switch when the 
 * level goal is met, and handles level restarts and menu navigation.
 * * B. New component & functionality learned: 
 * - Learned the Singleton pattern (public static UIManager instance) to allow 
 * other scripts to easily read the isMissionComplete variable without needing 
 * direct GameObject references.
 * - Utilized TextMeshProUGUI for high-quality, crisp UI text rendering.
 * * C. Problems encountered: 
 * - When restarting the level after a game over, the player was spawning with 
 * their previous low health. Solved this by deleting the "SavedHealth" PlayerPref 
 * inside the RestartLevel method.
 * * D. What you have tried / not tried: 
 * - Tried dynamically changing the completion text based on the active scene's name 
 * so Level 3 has a unique message compared to other levels.
 * - Have not tried adding UI animations (like text scaling or fading) when a kill 
 * is registered.
 * * E. Other important developer notes: 
 * - The Awake() method is used to set the instance variable to ensure it is fully 
 * initialized before any other scripts try to call it during their Start() methods.
 * ============================================================================== */

public class UIManager : MonoBehaviour
{
    // Singleton instance allowing global access to this script
    public static UIManager instance;

    [Header("UI Text Elements")]
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI killCountText;

    [Header("Mission Stats")]
    public int targetKills = 6;
    private int currentKills = 0;

    // --- NEW: A public switch to tell the rest of the game the mission is done! ---
    public bool isMissionComplete = false;

    // Awake is called before Start, making it ideal for initializing Singletons
    void Awake()
    {
        if (instance == null) { instance = this; }
    }

    void Start()
    {
        // Initialize the UI text immediately when the scene loads
        UpdateUI();
    }

    // Called by Enemy scripts when they are destroyed
    public void AddKill()
    {
        currentKills++;
        UpdateUI();

        // Check if the player has met the level requirements
        if (currentKills >= targetKills)
        {
            // --- NEW: Check which level we are currently in! ---
            string currentScene = SceneManager.GetActiveScene().name;

            // IMPORTANT: Make sure "Level 3" perfectly matches your scene file name
            if (currentScene == "Level 3")
            {
                missionText.text = "Mission Complete! Proceed to the door to go inside!";
            }
            else
            {
                missionText.text = "Mission Complete! Go to the stairs to proceed to the next level!";
            }
            // ---------------------------------------------------

            // Change text color to provide visual feedback of success
            missionText.color = Color.green;

            // Flip the switch to true! (The wall/door will be looking for this)
            isMissionComplete = true;
        }
    }

    // Refreshes the on-screen text to match current integer values
    private void UpdateUI()
    {
        if (currentKills < targetKills)
        {
            missionText.text = "Mission: Defeat " + targetKills + " Enemies";
        }
        killCountText.text = "Kills: " + currentKills + " / " + targetKills;
    }

    // Tied to UI Buttons (like a 'Retry' button on a Game Over screen)
    public void RestartLevel()
    {
        // Ensure time is flowing normally just in case the game was paused
        Time.timeScale = 1f;

        // Erase the saved health so you start Level 1 at full health!
        PlayerPrefs.DeleteKey("SavedHealth");
        PlayerPrefs.Save();

        // Hard-code this to your exact Level 1 scene name!
        SceneManager.LoadScene("Level 1");
    }

    // Tied to UI Buttons (like a 'Main Menu' button)
    public void GoToMainMenu()
    {
        // Ensure time is flowing normally
        Time.timeScale = 1f;

        // Erase the saved health here too, just in case!
        PlayerPrefs.DeleteKey("SavedHealth");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }
}