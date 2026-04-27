using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    // 🔹 1. ENUM (Top of script)
    public enum UIPage
    {
        MainMenu,
        Scan,
        ARView,
        OperationView,
    }

    // 🔹 2. UI References (Inspector)
    [Header("UI Pages")]
    public UIDocument mainMenu;
    public UIDocument scanUI;
    public UIDocument arView;
    public UIDocument operationView;


    // 🔹 3. Current State
    public UIPage currentPage;

    // 🔹 4. Internal Mappings
    private Dictionary<UIPage, UIDocument> pageUI;
    private Dictionary<UIPage, UIPage?> parentMap;

    // 🔹 5. Initialization
    void Awake()
    {
        // Map UIPage → UIDocument
        pageUI = new Dictionary<UIPage, UIDocument>()
        {
            { UIPage.MainMenu, mainMenu },
            { UIPage.Scan, scanUI },
            { UIPage.ARView, arView },
            { UIPage.OperationView, operationView },
        };

        // Define hierarchy (VERY IMPORTANT)
        parentMap = new Dictionary<UIPage, UIPage?>()
        {
            { UIPage.MainMenu, null },
            { UIPage.Scan, UIPage.MainMenu },
            { UIPage.ARView, UIPage.Scan } ,
            { UIPage.OperationView, UIPage.ARView },
        };
    }

    void OnEnable()
    {
        EventBus.OnARObjectSpawned += HandleARObjectSpawned;
        EventBus.OnUIPageChangeRequested += ShowPage;
    }

    void OnDisable()
    {
        EventBus.OnARObjectSpawned -= HandleARObjectSpawned;
        EventBus.OnUIPageChangeRequested -= ShowPage;
    }

    private void HandleARObjectSpawned(GameObject arObject)
    {
        ShowPage(UIPage.ARView);
    }

    // 🔹 6. Show Page
    public void ShowPage(UIPage page)
    {
        foreach (var ui in pageUI.Values)
            ui.rootVisualElement.style.display = DisplayStyle.None;

        pageUI[page].rootVisualElement.style.display = DisplayStyle.Flex;
        currentPage = page;
        EventBus.PublishUIPageChanged(page);
    }

    // 🔹 7. Back Navigation
    public void GoBack()
    {
        UIPage? parent = parentMap[currentPage];

        if (parent == null)
        {
            Application.Quit();
            return;
        }

        ShowPage(parent.Value);
    }
}

