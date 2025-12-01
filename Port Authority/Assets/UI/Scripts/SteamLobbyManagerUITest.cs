using Edgegap;
using IO.Swagger.Model;
using Mirror;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class SteamLobbyManagerUITest : MonoBehaviour
{
    public static SteamLobbyManagerUITest instance;
    public static Steamworks.Data.Lobby Lobby { get; private set; }

    public static bool isLobbySet = false;

    public static SteamId currentHostID;

    Steamworks.ServerList.Internet Request;

    public UnityEvent OnLobbyCreatedEvent;
    public UnityEvent OnLobbyJoinedEvent;
    public UnityEvent OnLobbyLeftEvent;

    public Button startButton;

    private readonly int lobbyListDelayDuration = 300; // original 3000

    public Dictionary<SteamId, Steamworks.Data.Lobby> lobbyData = new Dictionary<SteamId, Steamworks.Data.Lobby>();

    public Dictionary<SteamId, PlayerInfo> inLobby = new Dictionary<SteamId, PlayerInfo>();

    public Dictionary<SteamId, LobbyEntry> lobbyList = new Dictionary<SteamId, LobbyEntry>();

    private bool isAllReady = false;

    public NetworkPlayer localNetworkPlayer;

    private Steamworks.Data.LobbyQuery steamLobbyList;

    [Header("Lobby List UI")]
    public LobbyEntry lobbyEntryPrefab;
    public Transform lobbyListContainer;
    public TMP_Text noLobbyText;

    [Header("Selected Lobby UI")]
    public GameObject playerCardPrefab;
    public Transform playersGrid;
    public GameObject waitingCardPrefab;

    [Header("Joined Lobby UI")]
    public GameObject multiplayerMenuScreen;
    public GameObject joinedLobbyScreen;
    public GameObject selectedLobbySection;
    public TMP_Text joinedLobbyNameText;
    public TMP_Text joinedHostNameText;
    public Transform joinedLobbyContainer;


    public PlayerCard joinedPlayerCardPrefab;
    public Transform joinedPlayersGrid;

    public GameObject inviteCardPrefab;
    public Button joinedLobbyReadyButton;

    [Header("Color Choice UI")]
    public Transform colorChoicesContainer;
    public List<ColorChoice> colorChoices;
    public Transform newLobbyColorChoicesContainer;
    public List<ColorChoice> newLobbyColorChoices;

    [Header("Create Lobby UI")]
    public TMP_InputField lobbyNameInput;
    public TMP_InputField lobbySizeInput;

    private SteamId selectedLobbyId = 0;
    private ColorChoice selectedColorChoice = null;
    private ColorChoice newLobbyColorChoice = null;
    private float fadeDuration = 0.2f;
    private float popInDuration = 0.2f;
    private UnityEngine.Color readyColor;
    private UnityEngine.Color notReadyColor;
    private bool joiningCreatedLobby = false;

    private Button hostButton;
    private Button refreshButton;
    private Button createLobbyButton;
    private Button leaveButton;
    private Button multiplayerButton;

    private Transform hostDisconnectedBox;
    private Transform lobbyBox;
    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject); // destroy duplicates
            return;
        }

    }

    public void InitializeMenu(ReferenceGrab refGrab)
    {
        GetReferences(refGrab);
        RemoveCallbacks();
        selectedLobbyId = 0;
        selectedColorChoice = null;
        newLobbyColorChoice = null;
        joiningCreatedLobby = false;

        foreach (var friend in inLobby.Values)
        {
            Destroy(friend.playerObj);
        }

        lobbyData.Clear();
        inLobby.Clear();
        lobbyList.Clear();
        isAllReady = false;
        localNetworkPlayer = null;
        steamLobbyList = default;
        Lobby = default;
        currentHostID = default;
        GameObject[] oldPlayerObjects = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < oldPlayerObjects.Length; i++)
        {
            Destroy(oldPlayerObjects[i]);
        }


        //Request = new Steamworks.ServerList.Internet();
        //Request.RunQueryAsync(30);
        startButton.interactable = false;

        //attach functions to event listeners

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyMemberDataChanged += SetReadyStatus;
        SteamMatchmaking.OnLobbyMemberDataChanged += SetColor;

        //newLobbyColorChoice = newLobbyColorChoices[0];
        StartCoroutine(SetDefaultColors());

        // ui colors
        readyColor = new UnityEngine.Color(22f / 255f, 218f / 255f, 35f / 255f, 1f);
        notReadyColor = new UnityEngine.Color(157f / 255f, 157f / 255f, 157f / 255f, 1f);
        print(" test: " + createLobbyButton.name);
        hostButton.onClick.AddListener(Host);
        createLobbyButton.onClick.AddListener(OpenCreatePrompt);
        refreshButton.onClick.AddListener(GetLobbyInfo);
        startButton.onClick.AddListener(ClearLobbyForStart);
        leaveButton.onClick.AddListener(LeaveLobby);
        leaveButton.onClick.AddListener(GetLobbyInfo);
        multiplayerButton.onClick.AddListener(GetLobbyInfoWithoutFade);

        NetworkLobby networkLobby = NetworkRoomManager.singleton.gameObject.GetComponent<NetworkLobby>();

        if (networkLobby != null)
        {
            startButton.onClick.AddListener(NetworkRoomManager.singleton.gameObject.GetComponent<NetworkLobby>().MoveToGameplayScene);
        }
        else
        {
            Debug.LogError("ERROR: network lobby is null and did not properly add listener of - MoveToGameplayScene");
        }
    }

    private void GetReferences(ReferenceGrab refs)
    {
        startButton = refs.startButton;
        lobbyEntryPrefab = refs.lobbyEntryPrefab;
        lobbyListContainer = refs.lobbyListContainer;
        noLobbyText = refs.noLobbyText;

        playerCardPrefab = refs.playerCardPrefab;
        playersGrid = refs.playersGrid;
        waitingCardPrefab = refs.waitingCardPrefab;

        multiplayerMenuScreen = refs.multiplayerMenuScreen;
        joinedLobbyScreen = refs.joinedLobbyScreen;
        selectedLobbySection = refs.selectedLobbySection;
        joinedLobbyNameText = refs.joinedLobbyNameText;
        joinedHostNameText = refs.joinedHostNameText;
        joinedLobbyContainer = refs.joinedLobbyContainer;


        joinedPlayerCardPrefab = refs.joinedPlayerCardPrefab;
        joinedPlayersGrid = refs.joinedPlayersGrid;

        inviteCardPrefab = refs.inviteCardPrefab;
        joinedLobbyReadyButton = refs.joinedLobbyReadyButton;

        colorChoicesContainer = refs.colorChoicesContainer;

        lobbyNameInput = refs.lobbyNameInput;
        lobbySizeInput = refs.lobbySizeInput;

        hostButton = refs.hostButton;
        refreshButton = refs.refreshButton;
        createLobbyButton = refs.createLobbyButton;
        leaveButton = refs.leaveButton;
        multiplayerButton = refs.multiplayerButton;

        hostDisconnectedBox = refs.hostDisconnectedBox;
        lobbyBox = refs.lobbyBox;
    }

    public void RemoveCallbacks()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyMemberDataChanged -= SetReadyStatus;
        SteamMatchmaking.OnLobbyMemberDataChanged -= SetColor;
        hostButton.onClick.RemoveAllListeners();
        createLobbyButton.onClick.RemoveAllListeners();
        refreshButton.onClick.RemoveAllListeners();
        startButton.onClick.RemoveAllListeners();
        leaveButton.onClick.RemoveAllListeners();
        multiplayerButton.onClick.RemoveAllListeners();

        NetworkLobby networkLobby = NetworkRoomManager.singleton.gameObject.GetComponent<NetworkLobby>();

        if (networkLobby != null)
        {
            startButton.onClick.RemoveListener(NetworkRoomManager.singleton.gameObject.GetComponent<NetworkLobby>().MoveToGameplayScene);
        }
        else
        {
            Debug.LogError("ERROR: network lobby is null and did not properly add listener of - MoveToGameplayScene");
        }
    }

    public void OpenCreatePrompt()
    {
        print("open prompt update");
        lobbyNameInput.text = SteamClient.Name + "'s Lobby";
        lobbySizeInput.text = "4";
    }

    private void OnColorClicked(ColorChoice color)
    {
        if (selectedColorChoice != null)
        {
            selectedColorChoice.Unselect();
        }

        // select new entry and display lobby
        selectedColorChoice = color;
        selectedColorChoice.Select();
    }

    private IEnumerator SetDefaultColors()
    {
        yield return null;
        if(newLobbyColorChoices.Count > 0) OnColorClicked(newLobbyColorChoices[0]);
        if(colorChoices.Count > 0) OnColorClicked(colorChoices[0]);
    }

    private void SetReadyStatus(Steamworks.Data.Lobby lobby, Friend friend)
    {
        if (!inLobby.ContainsKey(friend.Id)) return;

        string readyString = Lobby.GetMemberData(friend, "isReady");
        if (!string.IsNullOrEmpty(readyString))
        {
            bool readyStatus = bool.Parse(readyString);

            inLobby[friend.Id].IsReady = readyStatus;

            if (inLobby[friend.Id].playerCardObj != null)
            {
                inLobby[friend.Id].playerCardObj.UpdateReadyButton(readyStatus);
            }

            isAllReady = IsAllReady();

            startButton.interactable = isAllReady;

            if (isAllReady)
            {
                startButton.image.color = new UnityEngine.Color(22f / 255f, 218f / 255f, 35f / 255f, 1f);
            }
            else
            {
                startButton.image.color = new UnityEngine.Color(157f / 255f, 157f / 255f, 157f / 255f, 1f);
            }


            print("member data changed: " + readyStatus);
            inLobby[friend.Id].IsReady = bool.Parse(Lobby.GetMemberData(friend, "isReady"));
        }
    }

    private void SetColor(Steamworks.Data.Lobby lobby, Friend friend)
    {
        if (!inLobby.ContainsKey(friend.Id)) return;

        if (inLobby.TryGetValue(friend.Id, out PlayerInfo info))
        {
            string lineColor = lobby.GetMemberData(friend, "lineColor");

            if (!string.IsNullOrEmpty(lineColor))
            {
                info.playerCardObj.UpdateColorVisual(lineColor);
            }
        }

    }

    public bool IsAllReady()
    {
        foreach (PlayerInfo playerInfo in inLobby.Values)
        {
            print(playerInfo.IsReady);
            if (!playerInfo.IsReady)
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
        lobbyData.Clear();

        foreach (Transform child in lobbyListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearLobbyForStart()
    {
        if (IsAllReady())
        {
            Lobby.SetData("hasStarted", "true");
            ClearLobby();
        }
    }
    /*
    void OnServersUpdated()
    {
        if (Request.Responsive.Count == 0)
        {
            print("No servers found currently");
            return;
        }

        foreach (var s in Request.Responsive)
        {
            ServerResponse(s);
        }

        Request.Responsive.Clear();
    }

    void ServerResponse(ServerInfo server)
    {
        Debug.Log($"{server.Name} Responded");
    }
    */
    public async void Host()
    {
        print("Hosting started...");
        var isSuccessful = await CreateLobby();

        if (!isSuccessful)
        {
            Debug.LogError("Error with creating lobby with steam.");
            return;
        }

        //await Task.Delay(5000);

        NetworkManager.singleton.StartHost();


    }

    public async void GetLobbyInfo() // refresh
    {

        StartCoroutine(RefreshCoroutineFadeOut());

        await Task.Delay(lobbyListDelayDuration);

        steamLobbyList = SteamMatchmaking.LobbyList;
        //var LobbyList = SteamMatchmaking.LobbyList;

        steamLobbyList = steamLobbyList.WithKeyValue("game", "PORTAUTH");
        steamLobbyList = steamLobbyList.WithKeyValue("hasStarted", "false");
        var LobbyResult = await steamLobbyList.RequestAsync();

        selectedLobbyId = 0;

        // clear old lobby entries
        ClearLobby();

        foreach (Transform child in lobbyListContainer)
        {
            Destroy(child.gameObject);
        }
        if(LobbyResult != null && LobbyResult.Length > 0)
        {
            foreach (var l in LobbyResult)
            {
                Debug.Log($"A lobby has been found: {l.GetData("name")} vs {Lobby.GetData("HostAddress")}.");
                Debug.Log(l.GetData("name"));

                LobbyEntry lobbyObj = Instantiate(lobbyEntryPrefab, lobbyListContainer);

                lobbyObj.lobbyNameText.text = l.GetData("name"); // name of lobby
                lobbyObj.lobbyNameText.ForceMeshUpdate();
                LayoutRebuilder.ForceRebuildLayoutImmediate(lobbyObj.lobbyNameText.rectTransform);
                int maxMembers = 4;
                int.TryParse(l.GetData("maxMembers"), out maxMembers);
                lobbyObj.countText.text = l.MemberCount + "/" + maxMembers; // count

                Button btn = lobbyObj.joinButton;
                btn.onClick.RemoveAllListeners();

                //btn.onClick.AddListener(() => OnLobbyClicked(l.Id, true));

                btn.onClick.AddListener(() =>
                {
                    AttemptJoin(l);
                    print("listener count: " + btn.onClick.GetPersistentEventCount());
                });

                lobbyList.Add(l.Id, lobbyObj);
                lobbyData.Add(l.Id, l);
                Debug.Log($"A lobby has been found: {l.GetData("name")} vs {Lobby.GetData("name")}.");

                if (selectedLobbyId == 0)
                {
                    selectedLobbyId = l.Id;
                }
            }
        }

        StartCoroutine(RefreshCoroutineFadeIn());
    }

    public async void GetLobbyInfoWithoutFade() // refresh
    {
        CanvasGroup listCg = lobbyListContainer.GetComponent<CanvasGroup>();
        listCg.alpha = 0;

        await Task.Delay(lobbyListDelayDuration);

        steamLobbyList = SteamMatchmaking.LobbyList;
        //var LobbyList = SteamMatchmaking.LobbyList;

        steamLobbyList = steamLobbyList.WithKeyValue("game", "PORTAUTH");
        steamLobbyList = steamLobbyList.WithKeyValue("hasStarted", "false");
        var LobbyResult = await steamLobbyList.RequestAsync();

        selectedLobbyId = 0;

        // clear old lobby entries
        ClearLobby();

        foreach (Transform child in lobbyListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var l in LobbyResult)
        {
            Debug.Log($"A lobby has been found: {l.GetData("name")} vs {Lobby.GetData("HostAddress")}.");
            Debug.Log(l.GetData("name"));

            LobbyEntry lobbyObj = Instantiate(lobbyEntryPrefab, lobbyListContainer);

            lobbyObj.lobbyNameText.text = l.GetData("name"); // name of lobby
            lobbyObj.lobbyNameText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(lobbyObj.lobbyNameText.rectTransform);
            int maxMembers = 4;
            int.TryParse(l.GetData("maxMembers"), out maxMembers);
            lobbyObj.countText.text = l.MemberCount + "/" + maxMembers; // count

            Button btn = lobbyObj.joinButton;
            btn.onClick.RemoveAllListeners();

            //btn.onClick.AddListener(() => OnLobbyClicked(l.Id, true));
            btn.onClick.AddListener(() => {
                AttemptJoin(l);
                print("listener count: " + btn.onClick.GetPersistentEventCount());
                });

            lobbyList.Add(l.Id, lobbyObj);
            lobbyData.Add(l.Id, l);
            Debug.Log($"A lobby has been found: {l.GetData("name")} vs {Lobby.GetData("name")}.");

            if (selectedLobbyId == 0)
            {
                selectedLobbyId = l.Id;
            }
        }

        StartCoroutine(RefreshCoroutineFadeIn());
    }


    private IEnumerator RefreshCoroutineFadeIn()
    {
        CanvasGroup listCg = lobbyListContainer.GetComponent<CanvasGroup>();

        // Handle empty lobby list
        if (lobbyList.Count == 0)
        {
            noLobbyText.gameObject.SetActive(true);
        }
        else
        {
            noLobbyText.gameObject.SetActive(false);

            yield return null;
        }

        yield return StartCoroutine(FadeIn(listCg));
    }

    private IEnumerator RefreshCoroutineFadeOut()
    {
        CanvasGroup listCg = lobbyListContainer.GetComponent<CanvasGroup>();

        yield return StartCoroutine(FadeOut(listCg));
    }

    private void AttemptJoin(Steamworks.Data.Lobby l)
    {
        int maxMembers = 4;
        int.TryParse(l.GetData("maxMembers"), out maxMembers);

        if (l.MemberCount < maxMembers && !IsInLobby(SteamClient.SteamId))
        {
            l.Join();
        }
        else
        {
            StartCoroutine(LobbyFullCoroutine(l));
        }
    }

    private bool IsInLobby(SteamId id)
    {
        print("-----------------------");
        foreach(var key in inLobby.Keys)
        {
            print("key: " + key);
        }
        print("-----------------------");
        return inLobby.ContainsKey(id);
    }

    private IEnumerator LobbyFullCoroutine(Steamworks.Data.Lobby l)
    {
        if (!lobbyList.TryGetValue(l.Id, out LobbyEntry entry))
            yield break;

        Button joinButton = entry.joinButton;
        TMP_Text buttonText = joinButton.GetComponentInChildren<TMP_Text>();

        UnityEngine.Color originalColor = joinButton.image.color;
        string originalText = buttonText.text;

        joinButton.image.color = UnityEngine.Color.red;
        buttonText.text = "Full";

        joinButton.interactable = false;

        yield return new WaitForSeconds(1f);

        joinButton.image.color = originalColor;
        buttonText.text = originalText;
        joinButton.interactable = true;
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

            // validate lobby name
            string lobbyName = lobbyNameInput.text.Trim();
            if (string.IsNullOrEmpty(lobbyName))
            {
                Debug.LogWarning("Lobby name cannot be empty!");
            }

            // validate lobby size
            int maxMembers = 4;
            if (!int.TryParse(lobbySizeInput.text, out maxMembers))
            {
                Debug.LogWarning("Invalid lobby size!");
            }

            SetLobby(lobbyOutput.Value);
            Lobby.SetPublic();
            // Lobby.SetFriendsOnly();
            Lobby.SetJoinable(true);
            Lobby.SetData("name", lobbyName);
            Lobby.SetData("maxMembers", maxMembers.ToString());
            Lobby.SetData("game", "PORTAUTH");
            Lobby.SetData("HostAddress", SteamClient.SteamId.Value.ToString());
            Lobby.SetData("hasStarted", "false");
            Lobby.SetData("hostID", SteamClient.SteamId.ToString());
            //SteamLobbyManagerUITest.currentHostID = SteamClient.SteamId;


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
        //if (inLobby.ContainsKey(friend.Id)) return;
        Debug.Log($"{friend.Name} joined the lobby");

        // clear waiting cards
        foreach (Transform child in joinedPlayersGrid)
        {
            if (child.gameObject.CompareTag("WaitingCard"))
            {
                Destroy(child.gameObject);
            }
        }


        PlayerCard playerObj = Instantiate(joinedPlayerCardPrefab, joinedPlayersGrid);

        List<Transform> popInCards = new List<Transform> { playerObj.transform };

        playerObj.playerNameText.text = friend.Name;

        Texture2D tex = await SteamProfileManager.GetTextureFromId(friend.Id);
        Sprite profileSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        playerObj.profilePicture.sprite = profileSprite;
        playerObj.profilePicture.type = UnityEngine.UI.Image.Type.Simple;

        PlayerInfo info = new PlayerInfo(playerObj);
        inLobby.Add(friend.Id, info);

        int maxMembers = 4;
        int.TryParse(lobby.GetData("maxMembers"), out maxMembers);

        // update ready
        string readyString = Lobby.GetMemberData(friend, "isReady");
        if (!string.IsNullOrEmpty(readyString))
        {
            bool readyStatus = bool.Parse(readyString);
            inLobby[friend.Id].IsReady = readyStatus;
            playerObj.UpdateReadyButton(readyStatus);
        }

        // update color
        string colorString = lobby.GetMemberData(friend, "lineColor");
        playerObj.UpdateColorVisual(string.IsNullOrEmpty(colorString) ? "Blue" : colorString);

        // update host
        var ownerId = lobby.Owner.Id;
        playerObj.UpdateHost(SteamClient.SteamId == ownerId);

        foreach (var player in inLobby.Values)
        {
            player.playerCardObj.UpdateHost(player.playerCardObj.playerNameText.text == lobby.Owner.Name);
        }

        int remaining = maxMembers - lobby.MemberCount;
        for (int i = 0; i < remaining; i++)
        {
            GameObject waitObj = Instantiate(waitingCardPrefab, joinedPlayersGrid);
        }

        isAllReady = IsAllReady();
        startButton.interactable = isAllReady;

        StartCoroutine(SequentialPopIn(popInCards));
    }

    void HostDisconnected()
    {
        print("host disconnected");
        hostDisconnectedBox.gameObject.SetActive(true);
        leaveButton.onClick.Invoke();
        //LeaveLobby();
        //GetLobbyInfo();
        //lobbyBox.gameObject.SetActive(false);
        //leaveButton.onClick.Invoke();
        //multiplayerButton.onClick.Invoke();
    }

    void OnLobbyMemberDisconnected(Steamworks.Data.Lobby lobby, Friend friend)
    {
        print(Lobby.Owner.Id + " : " + lobby.GetData("hostID"));
        if(Lobby.Owner.Id.ToString() != lobby.GetData("hostID"))
        {
            if(HostLeaveNotification.instance != null)
            {
                HostLeaveNotification.instance.HostLeft();
            }
            else
            {
                HostDisconnected();
            }
        }

        Debug.Log($"{friend.Name} left the lobby");
        //Debug.Log($"new lobby owner is {Lobby.Owner}");

        currentHostID = lobby.Owner.Id;


        if (inLobby.ContainsKey(friend.Id))
        {
            StartCoroutine(PopOut(inLobby[friend.Id].playerCardObj.gameObject.transform));
            inLobby.Remove(friend.Id);
        }
        /*
        var ownerId = lobby.Owner.Id;
        foreach (var member in inLobby)
        {
            bool isHost = (member.Key == ownerId);
            member.Value.playerCardObj.UpdateHost(isHost);
        }
        joinedHostNameText.text = "Host: " + lobby.Owner.Name;
        */

        if (SteamClient.SteamId == currentHostID)
        {
            startButton.gameObject.SetActive(true);
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }

        StartCoroutine(LobbyMemberDisconnectedCoroutine());
    }
    public IEnumerator PopOut(Transform target, float endScale = 0.8f)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 originalScale = target.localScale;
        target.localScale = originalScale;

        for (float t = 0; t < popInDuration; t += Time.deltaTime)
        {
            float factor = Mathf.SmoothStep(1f, endScale, t / popInDuration);
            target.localScale = originalScale * factor;
            yield return null;
        }

        target.localScale = originalScale * endScale;
        Destroy(target.gameObject);
        Instantiate(waitingCardPrefab, joinedPlayersGrid);
    }

    private IEnumerator LobbyMemberDisconnectedCoroutine()
    {
        CanvasGroup cg = joinedLobbyContainer.GetComponent<CanvasGroup>();

        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(joinedPlayersGrid.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectedLobbySection.GetComponent<RectTransform>());
    }

    void OnChatMessage(Steamworks.Data.Lobby lobby, Friend friend, string message)
    {
        Debug.Log($"Incoming message: {message} -- from user {friend.Name}");
    }


    async void OnGameLobbyJoinRequest(Steamworks.Data.Lobby lobby, SteamId id)
    {
        RoomEnter joinedLobbyStatus = await lobby.Join();

        if (joinedLobbyStatus != RoomEnter.Success)
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
        // get relevant canvas groups
        multiplayerMenuScreen.SetActive(false);
        joinedLobbyScreen.SetActive(true);
        CanvasGroup screenCg = joinedLobbyScreen.GetComponent<CanvasGroup>();

        SetLobby(lobby);
        Debug.Log("Client joined the lobby");

        CanvasGroup cg = joinedLobbyContainer.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        List<Transform> popInCards = new List<Transform>();


        // clear old players
        foreach (Transform child in joinedPlayersGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var friend in inLobby.Values)
        {
            Destroy(friend.playerObj);
        }

        inLobby.Clear();

        // set line color
        colorChoices = new List<ColorChoice>(colorChoicesContainer.GetComponentsInChildren<ColorChoice>());

        foreach (var colorChoice in colorChoices)
        {
            Button btn = colorChoice.GetComponent<Button>();
            btn.onClick.AddListener(() => SetUserLineColor(colorChoice.colorName));
            btn.onClick.AddListener(() => OnColorClicked(colorChoice));
        }

        OnColorClicked(colorChoices[0]);
        SetUserLineColor(colorChoices[0].colorName);

        Texture2D tex;
        Sprite profileSprite;
        var ownerId = lobby.Owner.Id;

        print("member Count: " + lobby.MemberCount);
        foreach (var friend in lobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                PlayerCard friendObj = Instantiate(joinedPlayerCardPrefab, joinedPlayersGrid);
                popInCards.Add(friendObj.transform);

                friendObj.playerNameText.text = friend.Name;

                // profile picture
                tex = await SteamProfileManager.GetTextureFromId(friend.Id);
                profileSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                friendObj.profilePicture.sprite = profileSprite;
                friendObj.profilePicture.type = UnityEngine.UI.Image.Type.Simple;

                PlayerInfo info = new PlayerInfo(friendObj);

                inLobby.Add(friend.Id, new PlayerInfo(friendObj));


                string readyString = Lobby.GetMemberData(friend, "isReady");

                if (!string.IsNullOrEmpty(readyString))
                {
                    bool readyStatus = bool.Parse(readyString);

                    inLobby[friend.Id].IsReady = readyStatus;

                    friendObj.UpdateReadyButton(readyStatus);
                }

                string colorString = lobby.GetMemberData(friend, "lineColor");
                if (!string.IsNullOrEmpty(colorString))
                {
                    friendObj.UpdateColorVisual(colorString);
                }
                else
                {
                    friendObj.UpdateColorVisual("Blue");
                }

                friendObj.UpdateHost(friend.Id == ownerId);
            }
        }

        // make object for current player
        PlayerCard playerObj = Instantiate(joinedPlayerCardPrefab, joinedPlayersGrid);

        joinedLobbyNameText.text = lobby.GetData("name");
        joinedHostNameText.text = "Host: " + lobby.Owner.Name;
        int maxMembers = 4;
        int.TryParse(lobby.GetData("maxMembers"), out maxMembers);

        // get profile picture
        tex = await SteamProfileManager.GetTextureFromId(SteamClient.SteamId);
        profileSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        playerObj.profilePicture.sprite = profileSprite;
        playerObj.profilePicture.type = UnityEngine.UI.Image.Type.Simple;
        playerObj.playerNameText.text = SteamClient.Name;

        // ready button
        Button readyBtn = playerObj.readyButton;
        readyBtn.gameObject.SetActive(true);
        playerObj.readyButton.image.color = notReadyColor;

        playerObj.isReady = false;
        playerObj.readyButton.gameObject.SetActive(true);

        readyBtn.onClick.AddListener(() =>
        {
            playerObj.isReady = !playerObj.isReady;
            ReadyPlayer(playerObj.isReady);
        });

        // keep track of player
        PlayerInfo playerInfo = new PlayerInfo(playerObj);

        playerInfo.onValueChanged.AddListener((bool ready) =>
        {
            playerObj.UpdateReadyButton(playerInfo.IsReady);
        });

        joinedLobbyReadyButton.onClick.RemoveAllListeners();
        joinedLobbyReadyButton.onClick.AddListener(() =>
        {
            playerInfo.IsReady = !playerInfo.IsReady;

            playerObj.UpdateReadyButton(playerInfo.IsReady);

            ReadyPlayer(playerInfo.IsReady);

            OnReadyButtonPressed(playerInfo.IsReady);
        });

        OnReadyButtonPressed(false);

        bool isHost = SteamClient.SteamId == ownerId;
        playerObj.UpdateHost(isHost);

        if (isHost)
        {
            startButton.gameObject.SetActive(true);
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }

        inLobby.Add(SteamClient.SteamId, playerInfo);
        popInCards.Add(playerObj.transform);

        // fill waiting slots
        int remaining = maxMembers - lobby.MemberCount;
        for (int i = 0; i < remaining; i++)
        {
            Instantiate(waitingCardPrefab, joinedPlayersGrid);
        }

        OnLobbyJoinedEvent.Invoke();

        string hostAddress = lobby.GetData("HostAddress");

        if (SteamClient.SteamId.ToString() != hostAddress)
        {
            Debug.Log($"Connecting to host {hostAddress} via Facepunch transport...");
            var manager = NetworkManager.singleton;
            manager.networkAddress = hostAddress;
            manager.StartClient();
        }
        else
        {
            Debug.Log("We are the host; no need to connect as client.");
        }

        print("Check IsAllReady status on joining: " + IsAllReady());

        StartCoroutine(SequentialPopIn(popInCards));
        StartCoroutine(DisplayJoinedLobbyCoroutine());
    }

    private IEnumerator DisplayJoinedLobbyCoroutine()
    {
        CanvasGroup cg = joinedLobbyContainer.GetComponent<CanvasGroup>();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(joinedPlayersGrid.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectedLobbySection.GetComponent<RectTransform>());
        yield return StartCoroutine(FadeIn(cg));
    }

    public void ReadyPlayer(bool status)
    {
        if (!isLobbySet) return;
        Lobby.SetMemberData("isReady", status.ToString());
        inLobby[SteamClient.SteamId].IsReady = status;

        print("check IsAllReady status on readying: " + IsAllReady());
    }

    public void OnReadyButtonPressed(bool ready)
    {
        if (ready)
        {
            joinedLobbyReadyButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            joinedLobbyReadyButton.image.color = new UnityEngine.Color(22f / 255f, 218f / 255f, 35f / 255f, 1f);
        }
        else
        {
            joinedLobbyReadyButton.GetComponentInChildren<TMP_Text>().text = "Not Ready";
            joinedLobbyReadyButton.image.color = new UnityEngine.Color(157f / 255f, 157f / 255f, 157f / 255f, 1f);
        }
    }

    public void SetUserLineColor(string colorName)
    {
        Lobby.SetMemberData("lineColor", colorName);

        if (inLobby.TryGetValue(SteamClient.SteamId, out PlayerInfo info))
        {
            info.playerCardObj.UpdateColorVisual(colorName);
        }
    }

    public void LeaveLobby()
    {
        Debug.LogError("CALLING BEFORE StopClient()   " +
            "NetworkClient.active=" + NetworkClient.active +
            "  isConnected=" + NetworkClient.isConnected +
            "  transport=" + NetworkManager.singleton.transport +
            "  networkAddress=" + NetworkManager.singleton.networkAddress);
        try
        {
            Lobby.Leave();
            OnLobbyLeftEvent.Invoke();
            NetworkManager.singleton.StopClient();

            /*
            NetworkManager.singleton.StopServer();
            NetworkManager.singleton.StopHost();

            NetworkServer.Shutdown();
            NetworkClient.Shutdown();

            NetworkManager.singleton.networkAddress = "";
            */


            Lobby = default;
            foreach (var friend in inLobby.Values)
            {
                Destroy(friend.playerObj);
            }

            inLobby.Clear();
        }
        catch(Exception err)
        {
            Debug.LogError("Error with leaving lobby: " + err);
        }
        Debug.LogError("CALLING AFTER StopClient()   " +
            "NetworkClient.active=" + NetworkClient.active +
            "  isConnected=" + NetworkClient.isConnected +
            "  transport=" + NetworkManager.singleton.transport +
            "  networkAddress=" + NetworkManager.singleton.networkAddress);
    }

    void OnLobbyInvite(Friend friend, Steamworks.Data.Lobby lobby)
    {
        Debug.Log($"{friend.Name} invited you to his/her game lobby");
    }

    // animations
    public IEnumerator FadeIn(CanvasGroup cg)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public IEnumerator FadeOut(CanvasGroup cg)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
    }

    public IEnumerator FadeIn(CanvasGroup cg1, CanvasGroup cg2)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg1.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            cg2.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg1.alpha = 1f;
        cg2.alpha = 1f;
    }

    public IEnumerator FadeOut(CanvasGroup cg1, CanvasGroup cg2)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg1.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            cg2.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg1.alpha = 0f;
        cg2.alpha = 0f;
    }


    public IEnumerator PopIn(Transform target, float startScale = 0.8f)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 originalScale = target.localScale;
        target.localScale = originalScale * startScale;

        for (float t = 0; t < popInDuration; t += Time.deltaTime)
        {
            if(target == null)
            {
                yield break;
            }

            float factor = Mathf.SmoothStep(startScale, 1f, t / popInDuration);
            target.localScale = originalScale * factor;
            yield return null;
        }

        target.localScale = originalScale;
    }

    public IEnumerator SequentialPopIn(List<Transform> cards, float delayBetween = 0.05f)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            StartCoroutine(PopIn(cards[i]));
            yield return new WaitForSeconds(delayBetween);
        }
    }

    private void OnApplicationQuit()
    {
        if(NetworkClient.active)
        {
            LeaveLobby();
        }
    }

}
