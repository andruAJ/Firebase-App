using Firebase.Auth;
using Firebase.Database;
using System;
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

    async void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        Username = await GetUsername();
        findMatchButton = uiDocument.rootVisualElement.Q<Button>("SearchMatchButton");
        findMatchButton.RegisterCallback<ClickEvent>(ev => OnFindMatchClicked());
        
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
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
            //aquí voy a llenar el panel de jugadores
            int i = 1;
            foreach (var player in dataSnapshot.Children)
            {
                uiDocument.rootVisualElement.Q<Label>("MatchPlayer"+i).text = player.Value.ToString();
                i++;
            }

            //aquí voy a emparejar aleatoriamente a los jugadores
            if (dataSnapshot.ChildrenCount >= 2)
            {
                var enumerator = dataSnapshot.Children.GetEnumerator();
                enumerator.MoveNext();
                string player1Id = enumerator.Current.Key;
                string player1Name = enumerator.Current.Value.ToString();
                enumerator.MoveNext();
                string player2Id = enumerator.Current.Key;
                string player2Name = enumerator.Current.Value.ToString();
                Debug.Log($"Partida encontrada entre {player1Name} y {player2Name}!");
                
                uiDocument.rootVisualElement.Q<Label>("NameRival").text = ;

                await mDatabaseRef.Child("SearchingForMatch").Child(currentUser.UserId).SetValueAsync(null);
                await mDatabaseRef.Child("SearchingForMatch").Child(player2Id).SetValueAsync(null);
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

}
