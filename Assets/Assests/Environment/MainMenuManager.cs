using UnityEngine;
using UnityEngine.SceneManagement;

/* * A. Functionality: Manages the Main Menu for Uytengsu Adventure. Connects the Play, Options, and Quit buttons.
 * B. New component & functionality learned: Used UnityEngine.SceneManagement for scene transitions and Application.Quit() to exit the build.
 * C. Problems encountered: The background image covered the buttons initially.
 * D. What you have tried / not tried: I learned that the Hierarchy order determines UI layering, so I moved the Background Image to the top of the Canvas list so buttons render on top of it.
 * E. Other important developer notes: Application.Quit() won't do anything in the Unity Editor; it only works when the game is fully built and exported!
 */
public class MainMenuManager : MonoBehaviour
{
    // 1. Connect to your PLAY button
    public void PlayGame()
    {
        SceneManager.LoadScene("Level 1"); // Make sure this is in Build Settings!
    }

    // 2. Connect to your OPTIONS button
    public void OpenOptions()
    {
        // Drop the breadcrumb saying we came from the MainMenu!
        // (Make sure "MainMenu" is the exact spelling of your Scene 1 file)
        PlayerPrefs.SetString("ReturnScene", "MainMenu");

        SceneManager.LoadScene("Game Setting");
    }

    // 3. Connect to your QUIT button
    public void QuitGame()
    {
        Debug.Log("Game is quitting... (Note: This only works in the final build, not in the editor!)");
        Application.Quit();
    }
}