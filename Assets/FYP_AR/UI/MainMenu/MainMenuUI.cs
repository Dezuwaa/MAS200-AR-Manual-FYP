using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private Button startButton;
    private Button exitButton;

    public UIManager uiManager;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startButton = root.Q<Button>("startButton");
        exitButton = root.Q<Button>("exitButton");

        startButton.clicked += OnStartClicked;
        exitButton.clicked += OnExitClicked;
    }

    void OnStartClicked()
    {
        Debug.Log("Clicked Works");

        if (uiManager == null)
        {
            Debug.LogError("UIManager NULL");
        }
        else
        {
            uiManager.ShowPage(UIManager.UIPage.Scan);    
        }
        
    }

    void OnExitClicked()
    {
        Application.Quit();
    }
}
