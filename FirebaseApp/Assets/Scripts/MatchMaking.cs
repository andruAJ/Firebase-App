using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MatchMaking : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button findMatchButton;

    private DatabaseReference mDatabaseRef;
    private string Username;

    private bool needRefresh = false;

    async void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        Username = await GetUsername();
        findMatchButton = uiDocument.rootVisualElement.Q<Button>("SearchMatchButton");
        findMatchButton.RegisterCallback<ClickEvent>(ev => OnFindMatchClicked());

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        var reference = FirebaseDatabase.DefaultInstance
        .GetReference("SearchingForMatch");

        reference.ChildAdded += HandleChildAdded;
        reference.ChildRemoved += HandleChildRemoved;
    }
    private async void OnFindMatchClicked()
    {
        Debug.Log("Buscando partida...");
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        await mDatabaseRef.Child("SearchingForMatch").Child(currentUser.UserId).SetValueAsync(Username);

        DatabaseReference usersRef = FirebaseDatabase.DefaultInstance.GetReference("SearchingForMatch");
        var dataSnapshot = await usersRef.GetValueAsync();
        

        if (dataSnapshot.Exists)
        {
            var playersList = new List<DataSnapshot>(dataSnapshot.Children);

            //aquí voy a emparejar aleatoriamente a los jugadores
            if (playersList.Count >= 2)
            {
                playersList.RemoveAll(p => p.Key == currentUser.UserId);

                if (playersList.Count == 0)
                {
                    Debug.Log("Solo estás tú buscando partida por ahora.");
                    return;
                }

                var rnd = new System.Random();
                int randomIndex = rnd.Next(playersList.Count);
                var opponentSnapshot = playersList[randomIndex];
                string opponentId = opponentSnapshot.Key;
                string opponentName = opponentSnapshot.Value?.ToString();
                Debug.Log($"Partida encontrada entre {Username} y {opponentName}!");
                
                uiDocument.rootVisualElement.Q<Label>("NameRival").text = opponentName;

                await mDatabaseRef.Child("SearchingForMatch").Child(currentUser.UserId).SetValueAsync(null);
                await mDatabaseRef.Child("SearchingForMatch").Child(opponentId).SetValueAsync(null);
            }
            else
            {
                Debug.Log("Esperando a más jugadores para emparejar...");
            }
        }
        else
        {
            Debug.Log("No hay usuarios en la base de datos ❗");
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
    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        needRefresh = true;
    }

    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        needRefresh = true;
    }
    private void OnApplicationQuit()
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        mDatabaseRef.Child("SearchingForMatch").Child(currentUser.UserId).SetValueAsync(null);
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
            var usersRef = FirebaseDatabase.DefaultInstance.GetReference("SearchingForMatch");
            var snapshot = await usersRef.GetValueAsync();
            //vaciar
            int i = 1;
            while (true)
            {
                var label = uiDocument.rootVisualElement.Q<Label>("MatchPlayer" + i);
                if (label == null) break;
                label.text = "";
                i++;
            }

            if (!snapshot.Exists) return;
            //rellenar
            i = 1;
            foreach (var child in snapshot.Children)
            {
                var label = uiDocument.rootVisualElement.Q<Label>("MatchPlayer" + i);
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
}
