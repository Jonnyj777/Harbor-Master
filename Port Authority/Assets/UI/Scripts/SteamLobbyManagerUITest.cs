using Edgegap;
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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class SteamLobbyManagerUITest : MonoBehaviour
{
    public static SteamLobbyManagerUITest instance;
    public static Steamworks.Data.Lobby Lobby { get; private set; }

    public static bool isLobbySet = false;
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


    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);

        Request = new Steamworks.ServerList.Internet();
        Request.RunQueryAsync(30);
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

        

        newLobbyColorChoice = newLobbyColorChoices[0];
        StartCoroutine(SetDefaultColors());

        // ui colors
        readyColor = new UnityEngine.Color(22f / 255f, 218f / 255f, 35f / 255f, 1f);
        notReadyColor = new UnityEngine.Color(157f / 255f, 157f / 255f, 157f / 255f, 1f);
    }

    public void OpenCreatePrompt()
    {
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
        OnColorClicked(newLobbyColorChoices[0]);
        OnColorClicked(colorChoices[0]);
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
            ClearLobby();
        }
    }

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

            String host = l.Owner.Name;
            

            //lobbyObj.hostText.text = "Host: " + l.Owner.Name; // host text


            Button btn = lobbyObj.joinButton;
            btn.onClick.RemoveAllListeners();

            btn.onClick.AddListener(() => OnLobbyClicked(l.Id, true));

            btn.onClick.AddListener(() => AttemptJoin(l));

            lobbyObj.hostText.text = "Host: " + host;

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

    public async void GetLobbyInfoWithoutFade() // refresh
    {
        CanvasGroup listCg = lobbyListContainer.GetComponent<CanvasGroup>();
        listCg.alpha = 0;

        await Task.Delay(lobbyListDelayDuration);

        steamLobbyList = SteamMatchmaking.LobbyList;
        //var LobbyList = SteamMatchmaking.LobbyList;

        steamLobbyList = steamLobbyList.WithKeyValue("game", "PORTAUTH");
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

            String host = l.Owner.Name;


            //lobbyObj.hostText.text = "Host: " + l.Owner.Name; // host text


            Button btn = lobbyObj.joinButton;
            btn.onClick.RemoveAllListeners();

            btn.onClick.AddListener(() => OnLobbyClicked(l.Id, true));

            btn.onClick.AddListener(() => AttemptJoin(l));

            lobbyObj.hostText.text = "Host: " + host;

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

    private void OnLobbyClicked(SteamId id, bool doFade = false)
    {
        //// unselect previous entry
        //if (lobbyList.TryGetValue(selectedLobbyId, out LobbyEntry oldEntry))
        //{
        //    oldEntry.Unselect();
        //}

        //// select new entry and display lobby
        //selectedLobbyId = id;

        //if (lobbyList.TryGetValue(id, out LobbyEntry newEntry))
        //{
        //    newEntry.Select();
        //}

        //StartCoroutine(DisplayLobbyCoroutine(id, doFade));
    }

    private void AttemptJoin(Steamworks.Data.Lobby l)
    {
        int maxMembers = 4;
        int.TryParse(l.GetData("maxMembers"), out maxMembers);

        if (l.MemberCount < maxMembers)
        {
            l.Join();
        }
        else
        {
            StartCoroutine(LobbyFullCoroutine(l));
        }
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

    void OnLobbyMemberDisconnected(Steamworks.Data.Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} left the lobby");
        Debug.Log($"new lobby owner is {Lobby.Owner}");

        if (inLobby.ContainsKey(friend.Id))
        {
            StartCoroutine(PopOut(inLobby[friend.Id].playerCardObj.gameObject.transform));
            
            inLobby.Remove(friend.Id);
        }

        var ownerId = lobby.Owner.Id;
        foreach (var member in inLobby)
        {
            bool isHost = (member.Key == ownerId);
            member.Value.playerCardObj.UpdateHost(isHost);
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
        readyBtn.onClick.AddListener(() =>
        {
            playerObj.isReady = !playerObj.isReady;
            ReadyPlayer(playerObj.isReady);
        });

        // keep track of player
        PlayerInfo playerInfo = new PlayerInfo(playerObj);

        playerObj.readyButton.gameObject.SetActive(true);
        //playerObj.UpdateReadyButton(playerInfo.IsReady);

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

}
