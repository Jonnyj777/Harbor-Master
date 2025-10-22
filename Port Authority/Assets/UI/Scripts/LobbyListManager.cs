using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListManager : MonoBehaviour
{
    [Header("Prefabs and UI")]
    public LobbyEntry lobbyEntryPrefab;
    public Transform lobbyListParent;

    [Header("Test Data")]
    public List<LobbyData> testLobbies = new List<LobbyData>();

    [Header("Selected Lobby UI")]
    public TMP_Text selectedLobbyNameText;
    public TMP_Text selectedHostNameText;
    public Transform selectedPlayersParent;
    public Transform selectedLobbyParent;
    public GameObject playerCardPrefab;
    public GameObject waitingCardPrefab;

    [Header("Color Data")]
    public List<NamedColor> colorList = new List<NamedColor>();
    private Dictionary<string, Color> colorLookup = new Dictionary<string, Color>();

    private GridLayoutGroup playersGrid;
    private LobbyEntry selectedLobbyEntry = null;

    void Awake()
    {
        playersGrid = selectedPlayersParent.GetComponent<GridLayoutGroup>();

        colorList = new List<NamedColor>
        {
            new NamedColor { name = "Blue",   hex = "#0EA5E9" },
            new NamedColor { name = "Pink",   hex = "#FF1F8B" },
            new NamedColor { name = "Orange", hex = "#FF7700" },
            new NamedColor { name = "Green",  hex = "#16DA23" },
            new NamedColor { name = "Purple", hex = "#B61BF3" },
            new NamedColor { name = "Yellow", hex = "#FFD900" },
            new NamedColor { name = "Red",    hex = "#E90E12" },
        };

        colorLookup.Clear();
        foreach (var nc in colorList)
        {
            if (ColorUtility.TryParseHtmlString(nc.hex, out Color parsed))
            {
                nc.color = parsed;
                if (!colorLookup.ContainsKey(nc.name))
                    colorLookup.Add(nc.name, parsed);
            }
            else
            {
                Debug.LogWarning($"Invalid color hex for {nc.name}: {nc.hex}");
            }
        }

        // Build sample lobbies
        testLobbies = new List<LobbyData>
        {
            new LobbyData
            {
                lobbyName = "Fun Lobby",
                id = 1,
                maxMembers = 4,
                host = new LobbyMember { name = "Alice", colorName = "Red" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Bob", colorName = "Green" }
                }
            },
            new LobbyData
            {
                lobbyName = "Casual Lobby",
                id = 2,
                maxMembers = 4,
                host = new LobbyMember { name = "Charlie", colorName = "Blue" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Dana", colorName = "Yellow" }
                }
            },
            new LobbyData
            {
                lobbyName = "Pro Players",
                id = 3,
                maxMembers = 5,
                host = new LobbyMember { name = "Eve", colorName = "Pink" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Frank", colorName = "Purple" },
                    new LobbyMember { name = "Grace", colorName = "Green" }
                }
            },
            new LobbyData
            {
                lobbyName = "Chill Zone",
                id = 4,
                maxMembers = 4,
                host = new LobbyMember { name = "Hank", colorName = "Orange" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Ivy", colorName = "Yellow" }
                }
            },
            new LobbyData
            {
                lobbyName = "Adventure Squad",
                id = 5,
                maxMembers = 4,
                host = new LobbyMember { name = "Jack", colorName = "Red" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Kim", colorName = "Blue" },
                    new LobbyMember { name = "Leo", colorName = "Green" }
                }
            },
            new LobbyData
            {
                lobbyName = "Night Owls",
                id = 6,
                maxMembers = 2,
                host = new LobbyMember { name = "Mia", colorName = "Purple" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Nina", colorName = "Blue" }
                }
            },
            new LobbyData
            {
                lobbyName = "Speedsters",
                id = 7,
                maxMembers = 6,
                host = new LobbyMember { name = "Oscar", colorName = "Yellow" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Pam", colorName = "Red" }
                }
            },
            new LobbyData
            {
                lobbyName = "Gamers United",
                id = 8,
                maxMembers = 4,
                host = new LobbyMember { name = "Quinn", colorName = "Green" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Ray", colorName = "Blue" },
                    new LobbyMember { name = "Sara", colorName = "Pink" }
                }
            },
            new LobbyData
            {
                lobbyName = "Elite Squad",
                id = 9,
                maxMembers = 3,
                host = new LobbyMember { name = "Tom", colorName = "Blue" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Uma", colorName = "Yellow" }
                }
            },
            new LobbyData
            {
                lobbyName = "Casual Gamers",
                id = 10,
                maxMembers = 4,
                host = new LobbyMember { name = "Vera", colorName = "Red" },
                members = new List<LobbyMember>
                {
                    new LobbyMember { name = "Will", colorName = "Blue" },
                    new LobbyMember { name = "Xander", colorName = "Green" }
                }
            }
        };

        //Ensure host is also included as a member
        foreach (var lobby in testLobbies)
        {
            if (lobby.host != null && !lobby.members.Exists(m => m.name == lobby.host.name))
            {
                lobby.members.Insert(0, lobby.host);
            }
        }
    }

    void Start()
    {
        LobbyMember.LobbyListManagerInstance = this;
        StartCoroutine(InitializeLobbies());
    }

    private IEnumerator InitializeLobbies()
    {
        LobbyEntry firstEntry = PopulateLobbies(testLobbies);

        // Wait one frame to ensure the layout and objects are ready
        yield return null;

        if (firstEntry != null && testLobbies.Count > 0)
        {
            Debug.Log($"Selecting first lobby: {testLobbies[0].lobbyName}");
            selectedLobbyEntry = firstEntry;
            selectedLobbyEntry.SetSelected();
            DisplaySelectedLobby(testLobbies[0]);
        }
        else
        {
            Debug.LogWarning("No firstEntry found after PopulateLobbies()");
        }
    }


    public LobbyEntry PopulateLobbies(List<LobbyData> lobbies)
    {
        LobbyEntry firstEntry = null;

        foreach (var lobby in lobbies)
        {
            LobbyEntry entry = Instantiate(lobbyEntryPrefab, lobbyListParent);
            entry.lobbyNameText.text = lobby.lobbyName;
            entry.hostText.text = "Host: " + (lobby.host != null ? lobby.host.name : "Unknown");

            int memberCount = lobby.members?.Count ?? 0;
            entry.countText.text = memberCount + "/" + lobby.maxMembers;
            entry.SetUnselected();

            // Capture loop variables for button callback
            LobbyEntry capturedEntry = entry;
            LobbyData capturedLobby = lobby;

            Button entryButton = entry.GetComponent<Button>();
            entryButton.onClick.AddListener(() => OnLobbyClicked(capturedEntry, capturedLobby));

            if (firstEntry == null)
                firstEntry = entry;
        }

        return firstEntry;
    }


    private void OnLobbyClicked(LobbyEntry entry, LobbyData lobby)
    {
        if (selectedLobbyEntry != null)
            selectedLobbyEntry.SetUnselected();

        selectedLobbyEntry = entry;
        selectedLobbyEntry.SetSelected();

        DisplaySelectedLobby(lobby);
    }

    private void DisplaySelectedLobby(LobbyData lobby)
    {
        StartCoroutine(DisplayLobbyCoroutine(lobby));
    }

    private IEnumerator DisplayLobbyCoroutine(LobbyData lobby)
    {
        CanvasGroup cg = selectedLobbyParent.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = selectedLobbyParent.gameObject.AddComponent<CanvasGroup>();

        float fadeDuration = 0.25f;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;

        foreach (Transform child in selectedPlayersParent)
            Destroy(child.gameObject);

        selectedLobbyNameText.text = lobby.lobbyName;
        selectedHostNameText.text = "Host: " + (lobby.host != null ? lobby.host.name : "Unknown");

        // Now all members include the host already
        foreach (var member in lobby.members)
        {
            GameObject playerCardGO = Instantiate(playerCardPrefab, selectedPlayersParent);
            PlayerCard card = playerCardGO.GetComponent<PlayerCard>();

            Color memberColor = colorLookup.TryGetValue(member.colorName, out var c) ? c : Color.white;

            bool isHost = lobby.host != null && member.name == lobby.host.name;
            card.SetPlayerInfo(member.name, member.colorName, memberColor, isHost);
        }

        // Fill waiting slots
        int remaining = lobby.maxMembers - lobby.members.Count;
        for (int i = 0; i < remaining; i++)
            Instantiate(waitingCardPrefab, selectedPlayersParent);

        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectedPlayersParent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectedLobbyParent.GetComponent<RectTransform>());

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}
