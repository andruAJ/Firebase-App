using Firebase.Auth;
using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FriendRequestManager : MonoBehaviour
{
    public static FriendRequestManager Instance { get; private set; }
    private DatabaseReference mDatabaseUsersRef;
    private string myUsername;
    private string myUserId;

    private string inboxRef =  "friendRequests/inbox"; 
    private string outboxRef = "friendRequests/outbox";

    public void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            Debug.Log("Hay mas instancias");
        }
        Instance = this;
    }

    async void Start()
    {
        mDatabaseUsersRef = FirebaseDatabase.DefaultInstance.GetReference("users");
        myUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        myUsername = await GetUsername();

        var inboxDatabaseRef =  mDatabaseUsersRef.Child(myUserId).Child(inboxRef);
        var outboxDatabaseRef = mDatabaseUsersRef.Child(myUserId).Child(outboxRef);

        inboxDatabaseRef.ChildAdded += HandleFriendRequestAdded;
        inboxDatabaseRef.ChildRemoved += HandleFriendRequestRemoved;

        outboxDatabaseRef.ChildAdded += HandleFriendResponseAdded;
        outboxDatabaseRef.ChildChanged += HandleFriendResponseChanged;
        outboxDatabaseRef.ChildRemoved += HandleFriendResponseRemoved;

    }

    public void SendFriendRequest(string friendUserId,string friendUsername)
    {

        //Referencia al inbox del usuario al que le quiero enviar la solicitud
        mDatabaseUsersRef.Child(friendUserId).Child(inboxRef).Child(myUserId).SetValueAsync(myUsername)
            .ContinueWith( task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error sending friend request: " + task.Exception);
                }
                else
                {
                    Debug.Log("Friend request sent to user: " + friendUserId);
                    string friendRequestJson = JsonUtility.ToJson(new FriendResponse
                    {
                        username = friendUsername,
                        status = 0 // pending
                    });
                    //Referencia al outbox del usuario que envia la solicitud
                    mDatabaseUsersRef.Child(myUserId).Child(outboxRef).Child(friendUserId).SetRawJsonValueAsync(friendRequestJson);
                }
            });
    }
    public void RespondFriendRequest(string friendUserId,string friendUsername ,int ResponseStatus)
    {
        //establecer el status de la solicitud en el outbox del usuario que envio la solicitud
        mDatabaseUsersRef.Child(friendUserId).Child(outboxRef).Child(myUserId).Child("status").SetValueAsync(ResponseStatus)
            .ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error accepting friend request: " + task.Exception);
                }
                else
                {
                    Debug.Log("The friend request from "+friendUserId+" has been responde with status "+ResponseStatus);

                    if (ResponseStatus == 1) // accepted
                    {
                        //Agregar al amigo a la lista de amigos
                        SaveFriend(friendUserId, friendUsername);
                    }
                    //Eliminar la solicitud del inbox del usuario que la recibio
                    mDatabaseUsersRef.Child(myUserId).Child(inboxRef).Child(friendUserId).SetValueAsync(null);

                }
            });
    }
    private void SaveFriend(string friendUserId,string friendUsername)
    {
        mDatabaseUsersRef.Child(myUserId).Child("friends").Child(friendUserId).SetValueAsync(friendUsername);
    }
    private async Task<string> GetUsername()
    {

        DataSnapshot usernameDataSnapshot = await FirebaseDatabase.DefaultInstance
           .GetReference("users/" + myUserId + "/username")
           .GetValueAsync();

        string username = "";
        if (usernameDataSnapshot != null)
        {
            username = (string)usernameDataSnapshot.Value;
        }

        return username;
    }

    //Outbox
    //Manejar la respuesta a la solicitud de amistad
    private void HandleFriendResponseChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        
       FriendResponse friendResponse = GetFriendResponseFromSnapshot(args.Snapshot);

       ProcessFriendResponse(friendResponse);
    }
    private void HandleFriendResponseAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }


        FriendResponse friendResponse = GetFriendResponseFromSnapshot(args.Snapshot);
        if(friendResponse.status == 0)
        {
            Debug.Log("Friend request to " + friendResponse.username + " is still pending.");
            return;
        }

        ProcessFriendResponse(friendResponse);

    }
    private void HandleFriendResponseRemoved(object sender, ChildChangedEventArgs args)
    {
        //Puedo usar este metodo para eliminar graficamente las peticiones respondidas
    }

    private FriendResponse GetFriendResponseFromSnapshot(DataSnapshot snapshot)
    {
        if (!snapshot.Exists)
            return null;
        var friendRequest = JsonUtility.FromJson<FriendResponse>(snapshot.GetRawJsonValue());
        friendRequest.userId = snapshot.Key;
        return friendRequest;
    }   

    private void ProcessFriendResponse(FriendResponse friendResponse)
    {
        if (friendResponse.status == 0)
            return;
        //Procesar la respuesta de la solicitud de amistad
        if (friendResponse.status == 1)
        {
            //La solicitud ha sido aceptada, agregar al amigo a la lista de amigos
            Debug.Log(" your friend request to " + friendResponse.username + " has been accepted.");
            SaveFriend(friendResponse.userId, friendResponse.username);
        }
        else if (friendResponse.status == 2)
        {
            //La solicitud ha sido rechazada
            Debug.Log(" your friend request to " + friendResponse.username + " has been rejected.");
        }
        //Eliminar la solicitud del outbox
        mDatabaseUsersRef.Child(myUserId).Child(outboxRef).Child(friendResponse.userId).SetValueAsync(null);
    }
    //Inbox
    //Manejar la llegada de nuevas solicitudes de amistad 
    private void HandleFriendRequestAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        if (!args.Snapshot.Exists)
            return;

        var friendUserId = args.Snapshot.Key;
        var friendUsername = (string)args.Snapshot.Value;
        Debug.Log("Friend request from "+ friendUsername+ ", userId " + args.Snapshot.Key);

        //Aqui puedo mostrar graficamente la solicitud de amistad entrante
    }
    private void HandleFriendRequestRemoved(object sender, ChildChangedEventArgs e)
    {
        //Aqui puedo eliminar graficamente la solicitud de amistad que ha sido respondida 
    }
}
public class FriendResponse
{
    public string userId;
    public string username;
    public int status; // 0 = pending, 1 = accepted, 2 = rejected
}
public class FriendRequest {
    public string userId;
    public string username;
}
