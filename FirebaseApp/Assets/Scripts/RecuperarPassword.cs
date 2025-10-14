using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class RecuperarPassword : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void ResetPassword()
    {
        FirebaseUser user = auth.CurrentUser;
        string email = user.Email;
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Hubo un error mandando el correo");
                return;
            }
        });

        //user.UpdatePasswordAsync().ContinueWith(task =>
        //{
        //    if (task.IsCanceled)
        //    {
        //        return;
        //    }
        //    if (task.IsFaulted)
        //    {
        //        return;
        //    }

        //});

    }
}
