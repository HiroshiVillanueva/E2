using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager functionality

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Handles the UI exit flow, allowing the player to transition from the ending 
 * screen back to the main menu or to completely close the application.
 * * B. New component & functionality learned: 
 * - Learned how to import UnityEngine.SceneManagement to use SceneManager.LoadScene() 
 * for navigating between different game states.
 * - Learned that scenes must be registered in the File > Build Profile list to load[cite: 36, 43].
 * - Discovered Application.Quit() to terminate the compiled executable.
 * * C. Problems encountered: 
 * - Returning to the menu sometimes left the game frozen because the timescale 
 * was still set to 0 (from a pause or game over state). Setting Time.timeScale = 1f fixed this.
 * - The Quit button appeared broken at first until I realized Application.Quit() 
 * is ignored inside the Unity Editor and only works in the final build.
 * * D. What you have tried / not tried: 
 * - I tried adding a Debug.Log to confirm the QuitGame method was actually triggering 
 * when the button was clicked via the On Click() inspector event[cite: 89].
 * - Have not tried adding a fade-out screen transition or a confirmation popup yet.
 * * E. Other important developer notes: 
 * - The string "MainMenu" passed into LoadScene must exactly match the name of the 
 * scene added to the Build Profile[cite: 36, 41].
 * ============================================================================== */

public class EndingExit : MonoBehaviour
{
    // Connect this to your "Back to Menu" button
    public void ReturnToMenu()
    {
        // Resets the time scale in case the game was paused prior to this scene
        Time.timeScale = 1f;
        // IMPORTANT: Make sure this exactly matches your Main Menu scene name!
        SceneManager.LoadScene("MainMenu");
    }

    // Connect this to your "Quit" button
    public void QuitGame()
    {
        Debug.Log("Quitting Game... (This only closes the game in a built .exe, not the Unity Editor!)");
        // Terminates the application when running a compiled version of the game
        Application.Quit();
    }
}