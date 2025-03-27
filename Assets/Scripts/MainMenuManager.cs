using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    public GameObject mainMenuPanel;
    public GameObject mapSelectPanel;
    public GameObject characterSelectPanel;

    private string selectedMap;
    private string selectedCharacter;

    public int playerCount { get; private set; } = 1;
    private HashSet<string> registeredInputs = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // optional
    }

    void Start()
    {
        registeredInputs.Add("Keyboard");
        ShowMainMenu();
    }

    void Update()
    {
           DetectNewPlayerInput();
    }

    private void DetectNewPlayerInput()
    {
                // Loop through possible joystick inputs
        for (int i = 1; i <= 4; i++) // Adjust based on max expected controllers
        {
            string joystickName = "Joystick" + i;

            if (!registeredInputs.Contains(joystickName))
            {
                // Detect if any button is pressed on this joystick
                if (Input.GetKeyDown("joystick " + i + " button 0") ||
                    Input.GetKeyDown("joystick " + i + " button 1") ||
                    Input.GetKeyDown("joystick " + i + " button 2") ||
                    Input.GetKeyDown("joystick " + i + " button 3"))
                {
                    registeredInputs.Add(joystickName);
                    playerCount++;
                    Debug.Log($"New player joined with {joystickName}. Total players: {playerCount}");
                }
            }
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        mapSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        mainMenuPanel.SetActive(false);
        mapSelectPanel.SetActive(true);
    }

    public void OnMapSelected(string mapName)
    {
        selectedMap = mapName;
        mapSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public void OnCharacterSelected(string characterName)
    {
        selectedCharacter = characterName;
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(selectedMap);
    }

    public string GetSelectedMap() => selectedMap;
    public string GetSelectedCharacter() => selectedCharacter;
}
