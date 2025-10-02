using UnityEngine;
using Steamworks;
using Mirror;
using System.Threading.Tasks;
using Edgegap;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SteamLobbyManager : MonoBehaviour
{
    public static Steamworks.Data.Lobby Lobby { get; private set; }

    public static bool isLobbySet = false;

    public UnityEvent OnLobbyCreatedEvent;
    public UnityEvent OnLobbyJoinedEvent;
    public UnityEvent OnLobbyLeftEvent;

    public GameObject friendTemplate;
    public Transform content;

    public Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();

    private void Start()
    {
        DontDestroyOnLoad(this);

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;

    }

    public void Host()
    {
        NetworkManager.singleton.StartHost();
        CreateLobbyAsync();
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
            Lobby.SetFriendsOnly();
            Lobby.SetJoinable(true);


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
        GameObject playerObj = Instantiate(friendTemplate, content);
        playerObj.GetComponent<TextMeshProUGUI>().text = friend.Name;
        playerObj.GetComponent<RawImage>().texture = await SteamProfileManager.GetTextureFromId(friend.Id);
        inLobby.Add(friend.Id, playerObj);
    }

    void OnLobbyMemberDisconnected(Steamworks.Data.Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} left the lobby");
        Debug.Log($"new lobby owner is {Lobby.Owner}");

        if (inLobby.ContainsKey(friend.Id))
        {
            Destroy(inLobby[friend.Id]);
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
            Destroy(friend);
        }

        inLobby.Clear();

        GameObject playerObj = Instantiate(friendTemplate, content);
        playerObj.GetComponentInChildren<TextMeshProUGUI>().text = SteamClient.Name;
        playerObj.GetComponentInChildren<RawImage>().texture = await SteamProfileManager.GetTextureFromId(SteamClient.SteamId);

        inLobby.Add(SteamClient.SteamId, playerObj);

        foreach (var friend in Lobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                GameObject friendObj = Instantiate(friendTemplate, content);
                friendObj.GetComponent<TextMeshProUGUI>().text = friend.Name;
                friendObj.GetComponent<RawImage>().texture = await SteamProfileManager.GetTextureFromId(friend.Id);
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
                Destroy(friend);
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
