using Firebase.Database;
using UnityEngine;
using UnityEngine.UIElements;

public class FriendsNotificationManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement notificaton;

    private DatabaseReference mDatabaseRef;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        notificaton = uiDocument.rootVisualElement.Q<VisualElement>("FriendOnlineNotification");

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        var reference = FirebaseDatabase.DefaultInstance
        .GetReference("users-online");

        reference.ChildAdded += HandleChildAdded;
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
            
        }
    }
}
