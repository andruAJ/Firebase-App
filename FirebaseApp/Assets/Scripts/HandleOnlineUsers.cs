using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class HandleOnlineUsers : MonoBehaviour
{
    [SerializeField] private List<GameObject> users;
    private DatabaseReference mDatabaseRef;
    private string Username;

    private bool needRefresh = false;

    async void Start() {

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        Username = await GetUsername();
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;

        var reference = FirebaseDatabase.DefaultInstance
        .GetReference("users-online");

        reference.ChildAdded += HandleChildAdded;
        reference.ChildRemoved += HandleChildRemoved;
    }

    private void HandleChildRemoved(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists) {
            Debug.Log(snapshot.Value + " se ha desconectado");
            needRefresh = true;
        }
    }

    private void HandleChildAdded(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists) {
            Debug.Log(snapshot.Value + " se ha conectado");
            needRefresh = true;
        }
    }

    private void HandleAuthStateChange(object sender, EventArgs e) {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;


        if (currentUser != null) {
            mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(Username);
        } else {
            mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(null);
        }
    }

    private async Task<string> GetUsername() {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        DataSnapshot usernameDataSnapshot = await FirebaseDatabase.DefaultInstance
           .GetReference("users/" + currentUser.UserId + "/username")
           .GetValueAsync();

        string username = "";
        if (usernameDataSnapshot != null) {
            username = (string)usernameDataSnapshot.Value;
        }
        
        return username;
    }
    private void Update() {
        if (needRefresh) {
            needRefresh = false;
            RefreshPlayerLabels();
        }
    }
    private async void RefreshPlayerLabels() {
        try {
            var usersRef = FirebaseDatabase.DefaultInstance.GetReference("users-online");
            var snapshot = await usersRef.GetValueAsync();
            //vaciar
            int i = 0;
            foreach (GameObject user in users) {
                var label = user.GetComponentInChildren<TextMeshProUGUI>();
                if (label == null) break;
                label.text = "";
                i++;
            }

            if (!snapshot.Exists) return;
            //rellenar
            i = 0;
            foreach (var child in snapshot.Children) {
                var label = users[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label == null) break;
                label.text = child.Value?.ToString() ?? "";
                users[i].GetComponentInChildren<UnityEngine.UI.Button>().
                    onClick.AddListener(() => 
                    FriendRequestManager.Instance.SendFriendRequest(child.Key, child.Value?.ToString()));
                users[i].SetActive(true);
                i++;
            }
        } catch (Exception ex) {
            Debug.LogError("Error al refrescar lista de jugadores: " + ex.Message);
        }
    }
    private void OnApplicationQuit() {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        mDatabaseRef.Child("users-online").Child(currentUser.UserId).SetValueAsync(null);
    }
}
