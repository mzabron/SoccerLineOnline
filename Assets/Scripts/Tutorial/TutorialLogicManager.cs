using UnityEngine;

public class TutorialLogicManager : LogicManager
{
    [Header("Tutorial Settings")]
    [SerializeField] private bool isTutorialMode = true;
    [SerializeField] private GameObject tutorialUI;
    
    protected override void Start()
    {
        base.Start();
        InitializeTutorial();
    }

    protected override void Update()
    {
        if (isTutorialMode)
        {
            if (player1Time < 999f) player1Time = 999f;
            if (player2Time < 999f) player2Time = 999f;
        }
        
        base.Update();
    }
    
    private void InitializeTutorial()
    {
        if (tutorialUI != null)
        {
            tutorialUI.SetActive(true);
        }
        
        Debug.Log("Tutorial initialized");
    }
}
