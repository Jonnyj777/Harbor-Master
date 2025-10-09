using UnityEngine;
using Steamworks;
using Mirror;
using System.Threading.Tasks;
using Edgegap;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using IO.Swagger.Model;
using Mirror.Discovery;
using Steamworks.Data;
using System;

public class SteamLobbyManager : MonoBehaviour
{
    public static Steamworks.Data.Lobby Lobby { get; private set; }

    public static bool isLobbySet = false;
    Steamworks.ServerList.Internet Request;

    public UnityEvent OnLobbyCreatedEvent;
    public UnityEvent OnLobbyJoinedEvent;
    public UnityEvent OnLobbyLeftEvent;

    public GameObject playerTemplate;
    public Transform playerContent;

    public GameObject lobbyTemplate;
    public Transform lobbyContent;

    public Button startButton;

    private readonly int lobbyListDelayDuration = 3000;

    public Dictionary<SteamId, PlayerInfo> inLobby = new Dictionary<SteamId, PlayerInfo>();

    public Dictionary<SteamId, GameObject> lobbyList = new Dictionary<SteamId, GameObject>();

    private bool isAllReady = false;


    private void Start()
    {
        DontDestroyOnLoad(this);

        Request = new Steamworks.ServerList.Internet();
        Request.RunQueryAsync(30);

        //attach functions to event listeners

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;

        GetLobbyInfo();

    }

    private bool IsAllReady()
    {
        foreach (PlayerInfo playerInfo in inLobby.Values)
        {
            print(playerInfo.isReady);
            if (!playerInfo.isReady)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearLobby()
    {
        foreach (var l in lobbyList.Values)
        {
            Destroy(l);
        }

        lobbyList.Clear();
    }

    public void ClearLobbyForStart()
    {
        if(IsAllReady())
        {
            ClearLobby();
        }
    }

    void OnServersUpdated()
    {
        if(Request.Responsive.Count == 0)
        {
            print("No servers found currently");
            return;
        }

        foreach(var s in Request.Responsive)
        {
            ServerResponse(s);
        }

        Request.Responsive.Clear();
    }

    void ServerResponse(ServerInfo server)
    {
        Debug.Log($"{server.Name} Responded");
    }
    public void Host()
    {
        NetworkManager.singleton.StartHost();
        CreateLobbyAsync();
    }

    public async void GetLobbyInfo()
    {
        //clear all lobbies that exist so no duplicates occur
        ClearLobby();

        await Task.Delay(lobbyListDelayDuration);
        var LobbyList = SteamMatchmaking.LobbyList;
        var LobbyResult = await LobbyList.RequestAsync();

        foreach(var l in LobbyResult)
        {
            if(l.GetData("game") == "PORTAUTH")
            {
                GameObject lobbyObj = Instantiate(lobbyTemplate, lobbyContent);
                Debug.Log(l.GetData("name"));
                lobbyObj.GetComponentInChildren<TextMeshProUGUI>().text = l.GetData("name");
                lobbyObj.GetComponentInChildren<Button>().onClick.AddListener(() => AttemptJoin(l));
                lobbyList.Add(l.Id, lobbyObj);
                Debug.Log($"A lobby has been found: {l.GetData("name")} vs {Lobby.GetData("name")}.");
            }
        }
    }

    private void AttemptJoin(Steamworks.Data.Lobby l)
    {
        l.Join();
    }

    public async void CreateLobbyAsync()
    {
        bool isSuccess = await CreateLobby();

        if (!isSuccess)
        {
            Debug.Log("Failed to create lobby");
        }
    }
    public void SetLobby(Steamworks.Data.Lobby newLobbyData)
    {
        Lobby = newLobbyData;
        isLobbySet = true;
        print("setLobby");
    }

    public async Task<bool> CreateLobby()
    {
        try
        {
            var lobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
            if (!lobbyOutput.HasValue)
            {
                Debug.Log("Created but not instantiated");
                return false;
            }

            SetLobby(lobbyOutput.Value);
            Lobby.SetPublic();
           // Lobby.SetFriendsOnly();
            Lobby.SetJoinable(true);
            Lobby.SetData("name", SteamClient.Name + "'s Lobby");
            Lobby.SetData("game", "PORTAUTH");
            //Lobby.


            return true;

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

    private void OnLobbyGameCreated(Steamworks.Data.Lobby lobby, uint ip, ushort port, SteamId id)
    {

    }

    private async void OnLobbyMemberJoined(Steamworks.Data.Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} joined the lobby");
        GameObject playerObj = Instantiate(playerTemplate, playerContent);
        playerObj.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
        playerObj.GetComponentInChildren<RawImage>().texture = await SteamProfileManager.GetTextureFromId(friend.Id);
        inLobby.Add(friend.Id, new PlayerInfo(playerObj));
    }

    void OnLobbyMemberDisconnected(Steamworks.Data.Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} left the lobby");
        Debug.Log($"new lobby owner is {Lobby.Owner}");

        if (inLobby.ContainsKey(friend.Id))
        {
            Destroy(inLobby[friend.Id].playerObj);
            inLobby.Remove(friend.Id);
        }
    }

    void OnChatMessage(Steamworks.Data.Lobby lobby, Friend friend, string message)
    {
        Debug.Log($"Incoming message: {message} -- from user {friend.Name}");
    }

    
    async void OnGameLobbyJoinRequest(Steamworks.Data.Lobby lobby, SteamId id)
    {
        RoomEnter joinedLobbyStatus = await lobby.Join();

        if(joinedLobbyStatus != RoomEnter.Success)
        {
            Debug.Log("failed to join lobby: " + joinedLobbyStatus);
        }
        else
        {
            SetLobby(lobby);
        }
    }
    

    void OnLobbyCreated(Result result, Steamworks.Data.Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log("lobby created failed: " + result);
        }
        else
        {
            OnLobbyCreatedEvent.Invoke();
            Debug.Log("lobby successfully created");
        }
    }

    async void OnLobbyEntered(Steamworks.Data.Lobby lobby)
    {
        Debug.Log("Client joined the lobby");

        foreach (var friend in inLobby.Values)
        {
            Destroy(friend.playerObj);
        }

        inLobby.Clear();

        GameObject playerObj = Instantiate(playerTemplate, playerContent);
        playerObj.GetComponentInChildren<TextMeshProUGUI>().text = SteamClient.Name;
        playerObj.GetComponentInChildren<RawImage>().texture = await SteamProfileManager.GetTextureFromId(SteamClient.SteamId);

        inLobby.Add(SteamClient.SteamId, new PlayerInfo(playerObj));

        foreach (var friend in Lobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                GameObject friendObj = Instantiate(playerTemplate, playerContent);
                friendObj.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
                friendObj.GetComponentInChildren<RawImage>().texture = await SteamProfileManager.GetTextureFromId(friend.Id);
                inLobby.Add(friend.Id, new PlayerInfo(friendObj));
            }
        }

        OnLobbyJoinedEvent.Invoke();
    }

    public void LeaveLobby()
    {
        try
        {
            Lobby.Leave();
            OnLobbyLeftEvent.Invoke();

            foreach (var friend in inLobby.Values)
            {
                Destroy(friend.playerObj);
            }

            inLobby.Clear();
        }
        catch
        {

        }
    }

    void OnLobbyInvite(Friend friend, Steamworks.Data.Lobby lobby)
    {
        Debug.Log($"{friend.Name} invited you to his/her game lobby");
    }
}
