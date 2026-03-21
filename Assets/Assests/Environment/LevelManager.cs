using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void OpenSettingsFromLevel()
    {
        // Drop the breadcrumb saying we came from Level 1!
        // IMPORTANT: Change "Scene4_Level1" to the EXACT name of your Level 1 scene file!
        PlayerPrefs.SetString("ReturnScene", "Level 1");

        // Now load the settings menu
        SceneManager.LoadScene("Game Setting");
    }
}