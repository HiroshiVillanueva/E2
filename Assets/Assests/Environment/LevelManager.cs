using UnityEngine;
using UnityEngine.SceneManagement;

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Manages the transition from a gameplay level to the settings menu. It records 
 * the current level's name so the settings menu knows exactly where to return the player.
 * * B. New component & functionality learned: 
 * - Learned how to use PlayerPrefs.SetString() to pass data between scenes. This is 
 * incredibly useful for maintaining state when switching active scenes.
 * - Reinforced UI button integration with the SceneManager.
 * * C. Problems encountered: 
 * - Before implementing PlayerPrefs, opening the settings from the game meant the 
 * "Back" button on the settings menu would always dump the player back to the Main Menu.
 * * D. What you have tried / not tried: 
 * - I tried using a global static variable to track the previous scene, but PlayerPrefs 
 * provided a quick, built-in solution.
 * - Have not tried using additive scene loading to overlay the settings UI on 
 * top of the paused game instead of loading a whole new scene.
 * * E. Other important developer notes: 
 * - "Level 1" is currently hardcoded here. If this script is attached to other levels, 
 * that string should be dynamically generated using SceneManager.GetActiveScene().name.
 * ============================================================================== */

public class LevelManager : MonoBehaviour
{
    // Connect this to the "Settings" or "Pause" button inside the level
    public void OpenSettingsFromLevel()
    {
        // Save the current level name so the Settings menu's "Back" button knows where to go
        PlayerPrefs.SetString("ReturnScene", "Level 1");

        // Now load the settings menu
        SceneManager.LoadScene("Game Setting");
    }
}