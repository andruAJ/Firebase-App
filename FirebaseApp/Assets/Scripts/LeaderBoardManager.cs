using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LeaderBoardManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement scoreTable;

    private void OnEnable()
    {
         ButtonLogout.OnLogout += GetLeaderBoard;
    }
    private void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        scoreTable = uiDocument.rootVisualElement.Q<VisualElement>("ScoreTable");
    }
    public void GetLeaderBoard()
    {
        Debug.Log("Getting Leaderboard...");
        int i = 1;
        FirebaseDatabase.DefaultInstance
          .GetReference("users").OrderByChild("score").LimitToLast(5)
          .GetValueAsync().ContinueWithOnMainThread(task => {
              if (task.IsFaulted)
              {

              }
              else if (task.IsCompleted)
              {
                  DataSnapshot snapshot = task.Result;
                  //Debug.Log(snapshot.Key);              
                  //Debug.Log(snapshot.Value);

                  var usuarios = (Dictionary<string, object>)snapshot.Value;
                  foreach (var usuarioDocumento in usuarios)
                  {
                      //Id del cada usuario
                      //Debug.Log(usuarioDocumento.Key);  

                      var usuario = (Dictionary<string, object>)usuarioDocumento.Value;
                      Debug.Log(usuario["username"] + "|" + usuario["score"]);
                      scoreTable.Q<Label>("User" + i + "NameText").text = usuario["username"].ToString();
                      scoreTable.Q<Label>("User" + i + "ScoreText").text = usuario["score"].ToString();
                      i++;

                  }


              }
          });
    }

}

