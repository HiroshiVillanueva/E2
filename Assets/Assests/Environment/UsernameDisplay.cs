using UnityEngine;
using TMPro; // Required for TextMeshPro

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Retrieves the player's saved username from system memory and displays it on 
 * the screen using a TextMeshPro UI element.
 * * B. New component & functionality learned: 
 * - Learned how to read saved string data using PlayerPrefs.GetString().
 * - Discovered PlayerPrefs.HasKey() to safely check if data exists before trying to load it.
 * * C. Problems encountered: 
 * - If the game was played for the very first time, the UI might show empty text 
 * or throw an error because no name was saved yet. Wrapping the load logic in HasKey fixed this.
 * * D. What you have tried / not tried: 
 * - Made RefreshName a public method instead of just putting the code directly in Start(), 
 * which allows the settings menu to force an instant UI update when the player saves.
 * - Have not tried adding a default "Guest" name if no key is found yet.
 * * E. Other important developer notes: 
 * - The TMPro namespace is strictly required at the top of the script to access 
 * and manipulate TextMeshProUGUI components via code.
 * ============================================================================== */

public class UsernameDisplay : MonoBehaviour
{
    // The UI text element where the name will appear
    public TextMeshProUGUI nameTextBox;

    void Start()
    {
        RefreshName(); // Grab the name when the level starts
    }

    // We can call this custom function whenever the player hits Save!
    public void RefreshName()
    {
        // Safely check if a username has actually been saved to prevent errors
        if (PlayerPrefs.HasKey("Username"))
        {
            // Retrieve the saved string and apply it to the TextMeshPro component
            string savedName = PlayerPrefs.GetString("Username");
            nameTextBox.text = savedName;
        }
    }
}