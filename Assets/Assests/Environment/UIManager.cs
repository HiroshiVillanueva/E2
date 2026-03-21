using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Text Elements")]
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI killCountText;

    [Header("Mission Stats")]
    public int targetKills = 6;
    private int currentKills = 0;

    // --- NEW: A public switch to tell the rest of the game the mission is done! ---
    public bool isMissionComplete = false;

    void Awake()
    {
        if (instance == null) { instance = this; }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddKill()
    {
        currentKills++;
        UpdateUI();

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

            missionText.color = Color.green;

            // Flip the switch to true! (The wall/door will be looking for this)
            isMissionComplete = true;
        }
    }

    private void UpdateUI()
    {
        if (currentKills < targetKills)
        {
            missionText.text = "Mission: Defeat " + targetKills + " Enemies";
        }
        killCountText.text = "Kills: " + currentKills + " / " + targetKills;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Erase the saved health so you start Level 1 at full health!
        PlayerPrefs.DeleteKey("SavedHealth");
        PlayerPrefs.Save();

        // Hard-code this to your exact Level 1 scene name!
        SceneManager.LoadScene("Level 1");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        // Erase the saved health here too, just in case!
        PlayerPrefs.DeleteKey("SavedHealth");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }
}