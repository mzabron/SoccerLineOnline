using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialLogicManager : LogicManager
{
    [Header("Tutorial Settings")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_SpriteAsset skipNextIcon;
    [SerializeField] private TMP_SpriteAsset skipPreviousIcon;

    private int stepNumber = 0;
    protected override void Start()
    {
        isTutorialMode = true;
        player1Nickname = PlayerPrefs.GetString("PlayerName", "Player 1");
        base.Start();
        tutorialUI.SetActive(true);
        UpdateTutorialStep();
    }

    protected override void Update()
    {
        base.Update();
    }

    public void OnSkipNextButtonClick()
    {
        stepNumber++;
        UpdateTutorialStep();
    }

    public void OnSkipPreviousButtonClick()
    {
        if (stepNumber > 0)
            stepNumber--;
        UpdateTutorialStep();
    }


    void UpdateTutorialStep()
    {
        switch (stepNumber)
        {
            case 0:
                descriptionPanel.SetActive(true);
                titleText.text = "Welcome to the Tutorial!";
                descriptionText.text = @"Here, you will learn everything you need to start playing. " +
                "You can navigate through the tutorial using the arrows <sprite=0> and <sprite=1> " +
                "Ready to start? Tap the right arrow to begin.";
                break;
            default:
                descriptionText.text = "Tutorial step not found.";
                break;
        }
    }
    
}
