using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UIElements;

public class RecuperarPassword : MonoBehaviour
{
    private UIDocument uiDocument;
    private TextField emailField;
    private Button cambiarCartaButton; 
    private Button resetButton;

    private FirebaseAuth auth;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        emailField = uiDocument.rootVisualElement.Q<TextField>("EmailRecuperar_TextField");
        cambiarCartaButton = uiDocument.rootVisualElement.Q<Button>("Cambiar_Button");
        resetButton = uiDocument.rootVisualElement.Q<Button>("Recuperar_Button");

        cambiarCartaButton.RegisterCallback<ClickEvent>(ev => {
            var loginCard = uiDocument.rootVisualElement.Q<VisualElement>("LogIn_Card");
            var recuperarCard = uiDocument.rootVisualElement.Q<VisualElement>("Recuperar_Card");
            loginCard.style.display = DisplayStyle.None;
            recuperarCard.style.display = DisplayStyle.Flex;
        });
        resetButton.RegisterCallback<ClickEvent>(ev => ResetPassword());

        auth = FirebaseAuth.DefaultInstance;
    }

    public void ResetPassword()
    {
        string email = emailField.text;
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Hubo un error mandando el correo");
                return;
            }
        });
    }
}
