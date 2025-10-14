using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonLogout : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button logoutButton;
    private VisualElement scoreCard;
    private VisualElement upperVisuals;

    public static event Action OnLogout;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        upperVisuals = uiDocument.rootVisualElement.Q<VisualElement>("VisualContainer");
        logoutButton = uiDocument.rootVisualElement.Q<Button>("LogOut");
        scoreCard = uiDocument.rootVisualElement.Q<VisualElement>("ScoreTable");
        logoutButton.RegisterCallback<ClickEvent>(ev => OnPointerClick());
    }
    public void OnPointerClick()
    {
        upperVisuals.style.display = DisplayStyle.None;
        scoreCard.style.display = DisplayStyle.Flex;
        OnLogout?.Invoke();
        FirebaseAuth.DefaultInstance.SignOut();
    }
}