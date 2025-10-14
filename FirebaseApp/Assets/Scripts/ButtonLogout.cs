using Firebase.Auth;
using Firebase.Database;
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

    private DatabaseReference mDatabaseRef;

    public static event Action OnLogout;

    public CompletePlayerController_NewInput player;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        upperVisuals = uiDocument.rootVisualElement.Q<VisualElement>("VisualContainer");
        logoutButton = uiDocument.rootVisualElement.Q<Button>("LogOut");
        scoreCard = uiDocument.rootVisualElement.Q<VisualElement>("ScoreTable");
        logoutButton.RegisterCallback<ClickEvent>(ev => OnPointerClick());
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }
    public void OnPointerClick()
    {
        upperVisuals.style.display = DisplayStyle.None;
        scoreCard.style.display = DisplayStyle.Flex;

        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser != null) 
        {
            int score = player.count;
            mDatabaseRef.Child("users").Child(currentUser.UserId).Child("score").SetValueAsync(score);
        }

        OnLogout?.Invoke();
        FirebaseAuth.DefaultInstance.SignOut();
    }
}