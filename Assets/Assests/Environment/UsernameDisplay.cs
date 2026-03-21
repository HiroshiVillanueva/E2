using UnityEngine;
using TMPro;

public class UsernameDisplay : MonoBehaviour
{
    public TextMeshProUGUI nameTextBox;

    void Start()
    {
        RefreshName(); // Grab the name when the level starts
    }

    // We can call this custom function whenever the player hits Save!
    public void RefreshName()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            string savedName = PlayerPrefs.GetString("Username");
            nameTextBox.text = savedName;
        }
    }
}