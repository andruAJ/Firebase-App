using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LabelUsername : MonoBehaviour
{
    private Label _label;

    private UIDocument uiDocument;

    //private void Reset()
    //{
    //    _label = GetComponent<TMP_Text>();
    //}
    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        _label = uiDocument.rootVisualElement.Q<Label>("Usuario");
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthChange;
    }

    private void HandleAuthChange(object sender, EventArgs e)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser != null)
        {
            SetLabelUsername(currentUser.UserId);
        }


    }

    private void SetLabelUsername(string UserId)
    {
        FirebaseDatabase.DefaultInstance
           .GetReference("users/" + UserId + "/username")
           .GetValueAsync().ContinueWithOnMainThread(task => {
               if (task.IsFaulted)
               {
                   Debug.Log(task.Exception);
                   _label.text = "NULL";
               }
               else if (task.IsCompleted)
               {
                   DataSnapshot snapshot = task.Result;
                   Debug.Log(snapshot.Value);
                   _label.text = (string)snapshot.Value;

               }
           });
    }


}

