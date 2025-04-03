using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject mapSelectPanel;
    public GameObject characterSelectPanel;
    public GameObject hoveredMap;

    public int playerCount;

    public string selectedMapName;

    
    [Header("Buttons")]
    public GameObject playButton;
    public GameObject Firstbutton;
    private GameObject lastSelectedMapObject;
    private GameObject firstCharacterObject;
    private GameObject lastSelectedCharacterObject;

    public void Start()
    {
        ShowMainMenu();   
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        mapSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(playButton);
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject != null)
            Debug.Log(currentSelectedGameObject.name);
            
    }

    void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != lastSelectedMapObject)
        {
            if (lastSelectedMapObject != null)
            {
                Image lastImage = lastSelectedMapObject.GetComponent<Image>();
                if (lastImage != null)
                    lastImage.color = Color.white; // Reset old selection
            }

            if (current != null)
            {
                Image currentImage = current.GetComponent<Image>();
                if (currentImage != null)
                    currentImage.color = Color.blue; // Highlight current selection
            }

            lastSelectedMapObject = current;
        }
    }

    public void OnPlayPressed()
    {
        mainMenuPanel.SetActive(false);
        mapSelectPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Firstbutton);
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject != null)
            Debug.Log(currentSelectedGameObject.name);
    }

    public void OnMapOneSelected()
    {
        selectedMapName = "Map1";
        Debug.Log("Map 1 selected");
        SelectCharacter();
    }
    public void OnMapTwoSelected()
    {
        selectedMapName = "Map2";
        Debug.Log("Map 2 selected");
        SelectCharacter();
    }
    public void OnMapThreeSelected()
    {
        selectedMapName = "Map3";
        Debug.Log("Map 3 selected");
        SelectCharacter();
    }

    public void SelectCharacter()
    {
        mapSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Firstbutton);
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject != null)
            Debug.Log(currentSelectedGameObject.name);
    }
}
