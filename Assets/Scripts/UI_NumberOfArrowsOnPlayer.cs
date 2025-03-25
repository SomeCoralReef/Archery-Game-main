using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;

public class UI_NumberOfArrowsOnPlayer : MonoBehaviour
{

    public TextMeshProUGUI text;
    private PlayerScript player;
    void Awake()
    {
        player = GetComponentInParent<PlayerScript>();
        text = GetComponent<TextMeshProUGUI>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = player.currentNumberofArrows.ToString();
    }
}
