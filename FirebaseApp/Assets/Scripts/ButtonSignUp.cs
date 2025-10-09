using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ButtonSignUp : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement signupCard;
    private Button signupButton;    //yo
    private Coroutine _registrationCoroutine;

    private DatabaseReference mDatabaseRef;

    void Reset()
    {
        signupCard = uiDocument.rootVisualElement.Q<VisualElement>("SignUp_Card");
        signupButton = signupCard.Q<Button>("SignUp_Button");
    }
    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        signupCard = uiDocument.rootVisualElement.Q<VisualElement>("SignUp_Card");
        signupButton = signupCard.Q<Button>("SignUp_Button");
        signupUsernameField = signupCard.Q<TextField>("Username_Register_TextField");
        signupPasswordField = signupCard.Q<TextField>("Password_Register_TextField");
        iniciarSesion = signupCard.Q<Button>("TienesCuenta_Button");
        _registrationButton.onClick.AddListener(HandleRegisterButtonClicked);
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void HandleRegisterButtonClicked()
    {
        string email = GameObject.Find("InputFieldEmail").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;


        _registrationCoroutine = StartCoroutine(RegisterUser(email, password));

    }

    private IEnumerator RegisterUser(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.IsCanceled)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");

        }
        else if (registerTask.IsFaulted)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + registerTask.Exception);
        }
        else
        {
            // Firebase user has been created.
            Firebase.Auth.AuthResult result = registerTask.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            var userId = result.User.UserId;
            var username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;

            mDatabaseRef.Child("users").Child(userId).Child("username").SetValueAsync(username);

        }

    }
}
