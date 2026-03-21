using UnityEngine;
using UnityEngine.SceneManagement;

/* * A. Functionality: Keeps the menu music playing continuously between Main Menu and Settings, but stops it when the level starts.
 * B. New component & functionality learned: Used DontDestroyOnLoad to keep an object alive across different scenes.
 * C. Problems encountered: Music would restart every time the player opened the settings menu, or it would keep playing during the gameplay.
 * D. What you have tried / not tried: Created a Singleton pattern to prevent duplicate audio tracks, and added an Update check to destroy the menu music when Level 1 loads.
 * E. Other important developer notes: Make sure the scene name in the Update function perfectly matches the Level 1 scene name!
 */
public class MenuMusicManager : MonoBehaviour
{
    private static MenuMusicManager instance;

    void Awake()
    {
        // 1. If we go back to the Main Menu, prevent a second song from overlapping the first one!
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 2. Make this music player immortal so it survives going to the Game Setting scene
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 3. If the player starts the game, destroy this menu music so the battle music can play!
        // IMPORTANT: Change "Scene4_Level1" to whatever your Level 1 scene is actually named!
        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            Destroy(gameObject);
        }
    }
}