using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
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

    private async void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            var usersRef = mDatabaseRef.Child("users").Child(currentUser.UserId).Child("friends");
            var friendssnapshot = await usersRef.GetValueAsync();

            foreach (var friend in friendssnapshot.Children)
            {
                if (friend.Key == snapshot.Key)
                {
                    string friendName = await mDatabaseRef.Child("users").Child(friend.Key).Child("username").GetValueAsync().ContinueWith(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            Debug.LogError("Error getting friend's username: " + task.Exception);
                            return "Unknown";
                        }
                        else
                        {
                            return task.Result.Value.ToString();
                        }
                    });
                    notificaton.style.display = DisplayStyle.Flex;
                    notificaton.Q<Label>("FR_Name").text = friendName;
                    await System.Threading.Tasks.Task.Delay(3000);
                    notificaton.style.display = DisplayStyle.None;
                }
            }
        }
    }
}
