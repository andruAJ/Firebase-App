using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class FriendRequestManager : MonoBehaviour
{
    public static FriendRequestManager Instance { get; private set; }
    private DatabaseReference mDatabaseUsersRef;
    private string myUsername;
    private string myUserId;

    private string inboxRef =  "friendRequests/inbox"; 
    private string outboxRef = "friendRequests/outbox";

    bool needRefresh = false;

    [SerializeField] private List<GameObject> outbox;
    [SerializeField] private List<GameObject> inbox;

    private UIDocument uiDocument;



    public void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            Debug.Log("Hay mas instancias");
        }
        Instance = this;
    }

    async void Start()
    {
        uiDocument = GetComponent<UIDocument>();

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

        RefreshPlayerLabels();
    }
    private void Update() {
        if (needRefresh) {
            RefreshPlayerLabels();
            needRefresh = false;
        }
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
                    needRefresh = true;
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
        mDatabaseUsersRef.Child(friendUserId).Child("friends").Child(myUserId).SetValueAsync(myUsername);
        mDatabaseUsersRef.Child(myUserId).Child(inboxRef).Child(friendUserId)?.SetValueAsync(null);
        mDatabaseUsersRef.Child(friendUserId).Child(outboxRef).Child(myUserId)?.SetValueAsync(null);
        needRefresh = true;
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
        needRefresh = true;
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
        needRefresh = true;
        ProcessFriendResponse(friendResponse);

    }
    private void HandleFriendResponseRemoved(object sender, ChildChangedEventArgs args)
    {
        //Puedo usar este metodo para eliminar graficamente las peticiones respondidas
        needRefresh = true;
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
            mDatabaseUsersRef.Child(myUserId).Child(outboxRef).Child(friendResponse.userId)?.SetValueAsync(null);
            mDatabaseUsersRef.Child(myUserId).Child(inboxRef).Child(friendResponse.userId)?.SetValueAsync(null);
            mDatabaseUsersRef.Child(friendResponse.userId).Child(inboxRef).Child(myUserId)?.SetValueAsync(null);
            mDatabaseUsersRef.Child(friendResponse.userId).Child(outboxRef).Child(myUserId)?.SetValueAsync(null);
        }
        //Eliminar la solicitud del outbox
        needRefresh = true;
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
        needRefresh = true;
    }
    private void HandleFriendRequestRemoved(object sender, ChildChangedEventArgs e)
    {
        //Aqui puedo eliminar graficamente la solicitud de amistad que ha sido respondida
        needRefresh = true;
    }

    private async void RefreshPlayerLabels() {
        try {
            // Outbox
            var outRef = mDatabaseUsersRef.Child(myUserId).Child(outboxRef);
            var outSnapshot = await outRef.GetValueAsync();
            foreach(GameObject obj in outbox) {
                obj.SetActive(false);
            }
            // Inbox
            var inRef = mDatabaseUsersRef.Child(myUserId).Child(inboxRef);
            var inSnapshot = await inRef.GetValueAsync();
            foreach (GameObject obj in inbox) {
                obj.SetActive(false);
            }
            int i = 0;
            int j = 0;

            if (outSnapshot.Exists) {
                foreach (var child in outSnapshot.Children) {
                    var label = outbox[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (label == null) break;
                    FriendResponse friendResponse = GetFriendResponseFromSnapshot(child);
                    friendResponse.status = 2;
                    label.text = friendResponse.username ?? "";
                    outbox[i].GetComponentInChildren<UnityEngine.UI.Button>().
                        onClick.AddListener(() =>
                        FriendRequestManager.Instance.ProcessFriendResponse(friendResponse));
                    outbox[i].SetActive(true);
                    i++;
                }
            } else if (inSnapshot.Exists) {
                foreach (var child in inSnapshot.Children) {
                    var label = inbox[j].GetComponentInChildren<TextMeshProUGUI>();
                    if (label == null) break;
                    FriendResponse response = new() {
                        userId = child.Key,
                        username = child.Value?.ToString(),
                        status = 1,
                    };
                    label.text = response.username ?? "";
                    inbox[j].transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().
                        onClick.AddListener(() =>
                        ProcessFriendResponse(response));
                    // buton Declinar
                    FriendResponse declineResponse = new() {
                        userId = child.Key,
                        username = child.Value?.ToString(),
                        status = 2,
                    };
                    inbox[j].transform.GetChild(1).GetComponent<UnityEngine.UI.Button>().
                        onClick.AddListener(() =>
                        ProcessFriendResponse(declineResponse));
                    inbox[j].SetActive(true);
                    j++;
                }
            } else {
                return;
            }
        } catch (Exception ex) {
            Debug.LogError("Error al refrescar lista de jugadores: " + ex.Message);
        }
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
