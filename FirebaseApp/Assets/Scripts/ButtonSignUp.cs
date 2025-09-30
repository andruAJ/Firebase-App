using UnityEngine;

public class ButtonSignUp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void HandleRegisterButtonClicked()
    {
        string email = GameObject.Find("InputEmail").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputPassword").GetComponent<TMP_InputField>().text;   //Tengo que cambiar esto para que funcione con el UI Toolkit

        _registrationCoroutine = StartCoroutine(RegisterUser(email, password));

    }
}
