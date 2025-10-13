using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonLogin : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement signupCard;
    private VisualElement loginCard;
    private Button loginButton;
    private TextField usernameField;
    private TextField passwordField;
    private Button registrarse;

    //void Reset()
    //{
    //    _loginButton = GetComponent<Button>();
    //    _emailInputField = GameObject.Find("InputFieldEmail").GetComponent<TMP_InputField>();
    //    _emailPasswordField = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>();
    //}
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        signupCard = uiDocument.rootVisualElement.Q<VisualElement>("SignUp_Card");
        loginCard = uiDocument.rootVisualElement.Q<VisualElement>("LogIn_Card");
        loginButton = loginCard.Q<Button>("LogIn_Button");
        usernameField = loginCard.Q<TextField>("Username_TextField");
        passwordField = loginCard.Q<TextField>("Password_TextField");
        registrarse = loginCard.Q<Button>("Registrarse_Button");

        loginButton.RegisterCallback<ClickEvent>(ev => HandleLoginButtonClicked());
        registrarse.RegisterCallback<ClickEvent>(ev => { loginCard.style.display = DisplayStyle.None; signupCard.style.display = DisplayStyle.Flex; });
    }

    private void HandleLoginButtonClicked()
    {
        var auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync(usernameField.text, passwordField.text).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }
}
