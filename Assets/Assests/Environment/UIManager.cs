using UnityEngine;
using TMPro; // This is required for TextMeshPro!

public class UIManager : MonoBehaviour
{
    // This makes it easy for ANY enemy to find this script without manual linking
    public static UIManager instance;

    [Header("UI Text Elements")]
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI killCountText;

    [Header("Mission Stats")]
    public int targetKills = 6;
    private int currentKills = 0;

    void Awake()
    {
        // Set up the singleton pattern
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // Update the text right when the game starts
        UpdateUI();
    }

    // Your enemies will call this method when they die!
    public void AddKill()
    {
        currentKills++;
        UpdateUI();

        // Check if the player won
        if (currentKills >= targetKills)
        {
            missionText.text = "Mission Complete!";
            missionText.color = Color.green; // Make it pop!
        }
    }

    private void UpdateUI()
    {
        // Keep the mission text updated (unless we already won)
        if (currentKills < targetKills)
        {
            missionText.text = "Mission: Defeat " + targetKills + " Enemies";
        }

        // Update the live kill counter
        killCountText.text = "Kills: " + currentKills + " / " + targetKills;
    }
}