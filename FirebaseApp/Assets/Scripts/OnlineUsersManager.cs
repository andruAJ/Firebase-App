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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        //reference.ChildChanged += HandleChildChanged;
        //reference.ChildMoved += HandleChildMoved;


        //FirebaseDatabase.DefaultInstance
        // .GetReference("users-online")
        // .ValueChanged += UsuarioOnlineEventHandler;


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
        }
    }

    //private void UsuarioOnlineEventHandler(object sender, ValueChangedEventArgs args)
    //{
    //    if (args.DatabaseError != null)
    //    {
    //        Debug.LogError(args.DatabaseError.Message);
    //        return;
    //    }
    //    DataSnapshot snapshot = args.Snapshot;


    //    if (snapshot.Exists)
    //    {
    //        var usersOnline = (Dictionary<string, object>)snapshot.Value;
    //        foreach (var user in usersOnline)
    //        {
    //            Debug.Log(user.Value);
    //        }
    //    }
        
    //}

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
    private void OnApplicationQuit()
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(null);
    }

}
