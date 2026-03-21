using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Connections")]
    public TMP_InputField usernameInput;
    public Slider volumeSlider;
    public AudioMixer mainMixer;

    [Header("Pause Menu Settings")]
    public bool isPauseMenu = false; // We will check this box in Level 1!
    public GameObject pauseCanvas;   // The UI to hide/show

    void Start()
    {
        // Load saved data when the menu opens
        if (PlayerPrefs.HasKey("Username")) { usernameInput.text = PlayerPrefs.GetString("Username"); }
        if (PlayerPrefs.HasKey("Volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            volumeSlider.value = savedVolume;
            mainMixer.SetFloat("MasterVolume", savedVolume);
        }
    }

    public void UpdateVolume(float sliderValue)
    {
        mainMixer.SetFloat("MasterVolume", sliderValue);
    }

    // NEW: Call this from your Level 1 Options Button!
    public void OpenPauseMenu()
    {
        pauseCanvas.SetActive(true); // Show the menu
        Time.timeScale = 0f;         // FREEZE THE GAME!
    }

    public void SaveAndGoBack()
    {
        PlayerPrefs.SetString("Username", usernameInput.text);
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();

        // --- UPDATED CODE: Using the new, faster Unity command! ---
        UsernameDisplay display = FindFirstObjectByType<UsernameDisplay>();

        if (display != null)
        {
            display.RefreshName();
        }
        // -----------------------------------------------------------

        if (isPauseMenu)
        {
            Time.timeScale = 1f;
            pauseCanvas.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    public void RestartLevel()
    {
        // 1. UNFREEZE TIME! (Crucial if restarting from a Pause menu)
        Time.timeScale = 1f;

        // 2. Find the name of the level we are currently in, and reload it
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void GoToMainMenu()
    {
        // 1. UNFREEZE TIME! (Crucial so your Main Menu animations don't freeze)
        Time.timeScale = 1f;

        // 2. Load the Main Menu scene 
        // IMPORTANT: Change "Scene1_MainMenu" to the exact spelling of your scene file!
        SceneManager.LoadScene("MainMenu");
    }
}