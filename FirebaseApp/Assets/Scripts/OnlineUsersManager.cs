using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class OnlineUsersManager : MonoBehaviour
{
    private UIDocument uiDocument;


    private DatabaseReference mDatabaseRef;
    private string Username;

    private bool needRefresh = false;

    async void Start()
    {
        uiDocument = GetComponent<UIDocument>();

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        Username = await GetUsername();
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;

        var reference = FirebaseDatabase.DefaultInstance
        .GetReference("users-online");

        reference.ChildAdded += HandleChildAdded;
        reference.ChildRemoved += HandleChildRemoved;
    }

    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            Debug.Log(snapshot.Value + " se ha desconectado");
            needRefresh = true;
        }
    }

    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            Debug.Log(snapshot.Value + " se ha conectado");
            needRefresh = true;
        }
    }

    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;


        if (currentUser != null)
        {
            mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(Username);
        }
        else
        {
            mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(null);
        }
    }

    private async Task<string> GetUsername()
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        DataSnapshot usernameDataSnapshot = await FirebaseDatabase.DefaultInstance
           .GetReference("users/" + currentUser.UserId + "/username")
           .GetValueAsync();

        string username = "";
        if (usernameDataSnapshot != null)
        {
            username = (string)usernameDataSnapshot.Value;
        }
       
        return username;
    }
    private void Update()
    {
        if (needRefresh)
        {
            needRefresh = false;
            RefreshPlayerLabels();
        }
    }
    private async void RefreshPlayerLabels()
    {
        try
        {
            var usersRef = FirebaseDatabase.DefaultInstance.GetReference("users-online");
            var snapshot = await usersRef.GetValueAsync();
            //vaciar
            int i = 1;
            while (true)
            {
                var label = uiDocument.rootVisualElement.Q<Label>("PlayerOnline" + i);
                if (label == null) break;
                label.text = "";
                i++;
            }

            if (!snapshot.Exists) return;
            //rellenar
            i = 1;
            foreach (var child in snapshot.Children)
            {
                var label = uiDocument.rootVisualElement.Q<Label>("PlayerOnline" + i);
                if (label == null) break;
                label.text = child.Value?.ToString() ?? "";
                i++;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al refrescar lista de jugadores: " + ex.Message);
        }
    }
    private void OnApplicationQuit()
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(null);
    }

}
