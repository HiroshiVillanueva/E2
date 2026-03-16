using UnityEngine;
using UnityEngine.SceneManagement; // We need this line to load scenes!

public class MainMenuController : MonoBehaviour
{
    // This function will be called by your Play Button
    public void PlayGame()
    {
        // Replace "Level1" with the exact name of your game scene
        SceneManager.LoadScene("Level 1 (test)"); 
    }

    // This function will be called by your Quit Button
    public void QuitGame()
    {
        // This prints a message in the console so you know it works in the Editor
        Debug.Log("Game is exiting!"); 
        
        // This actually quits the game when you build and play it for real
        Application.Quit(); 
    }
}