using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderBoardManager : MonoBehaviour
{
    private void Start()
    {
        GetLeaderBoard();
    }
    public void GetLeaderBoard()
    {
        FirebaseDatabase.DefaultInstance
          .GetReference("users").OrderByChild("score").LimitToLast(3)
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

                  }


              }
          });
    }

}

