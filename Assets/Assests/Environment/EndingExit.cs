using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingExit : MonoBehaviour
{
    // Connect this to your "Back to Menu" button
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        // IMPORTANT: Make sure this exactly matches your Main Menu scene name!
        SceneManager.LoadScene("MainMenu");
    }

    // Connect this to your "Quit" button
    public void QuitGame()
    {
        Debug.Log("Quitting Game... (This only closes the game in a built .exe, not the Unity Editor!)");
        Application.Quit();
    }
}