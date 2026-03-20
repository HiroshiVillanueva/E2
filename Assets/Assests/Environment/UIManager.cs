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
            missionText.text = "Mission Complete! Go to the stairs to proceed to the next level!";
            missionText.color = Color.green;

            // --- NEW: Flip the switch to true! (The wall will be looking for this) ---
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
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}