using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SliderManager : MonoBehaviour
{
    public Slider playerSlider; // Assign the slider in the Inspector
    

    public void OnPlayButtonPressed()
    {
        // Store the slider value using PlayerPrefs
        int playerCount = Mathf.RoundToInt(playerSlider.value); // Round the slider value to the nearest integer
        PlayerPrefs.SetInt("PlayerCount", playerCount);
        PlayerPrefs.Save();
        
        // Load the next scene
        SceneManager.LoadScene("TestBattleScene"); // Replace "GameScene" with your scene name
    }
}

