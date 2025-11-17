using Mirror;
using Steamworks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HostLobby : MonoBehaviour
{
    //public Steamworks.Data.Lobby hostedLobby;


    public void Awake()
    {
        //DontDestroyOnLoad(this);
    }
    public void Host()
    {
       // NetworkManager.singleton.StartHost();
        //CreateLobbyAsync();
        //SteamManager.Singleton.SetLobby(hostedLobby);
    }
/*
    public async void CreateLobbyAsync()
    {
        bool isSuccess = await CreateLobby();

        if (!isSuccess)
        {
            Debug.Log("Failed to create lobby");
        }
    }

    public async Task<bool> CreateLobby()
    {
        try
        {
            var lobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
            if(!lobbyOutput.HasValue)
            {
                Debug.Log("Created but not instantiated");
                return false;
            }

            hostedLobby = lobbyOutput.Value;
            hostedLobby.SetPublic();
            hostedLobby.SetFriendsOnly();
            hostedLobby.SetJoinable(true);

            return true;

        } 
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
*/
}
