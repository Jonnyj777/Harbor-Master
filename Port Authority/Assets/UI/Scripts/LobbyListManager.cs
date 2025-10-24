using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class LobbyMember
{
    public string name;
    public string id;
    public ColorChoice colorChoice; 
}

[System.Serializable]
public class LobbyData
{
    public string lobbyName;
    public int id;
    public int maxMembers;

    public LobbyMember host;             
    public List<LobbyMember> members;   
}

public class LobbyListManager : MonoBehaviour
{
    [Header("Test Data")]
    public List<LobbyData> lobbies = new List<LobbyData>();

    [Header("Lobby List UI")]
    public LobbyEntry lobbyEntryPrefab;
    public Transform lobbyListContainer;
    public TMP_Text noLobbyText;
    public TMP_Text noSelectedLobbyText;

    [Header("Selected Lobby UI")]
    public TMP_Text selectedLobbyNameText;
    public TMP_Text selectedHostNameText;
    public Transform selectedLobbyContainer;
    public Transform playersGrid;
    public GameObject playerCardPrefab;
    public GameObject waitingCardPrefab;

    [Header("Joined Lobby UI")]
    public TMP_Text joinedLobbyNameText;
    public TMP_Text joinedHostNameText;
    public Transform joinedLobbyContainer;
    public Transform joinedPlayersGrid;
    public GameObject joinedPlayerCardPrefab;
    public GameObject inviteCardPrefab;

    [Header("Color Choice UI")]
    public Transform colorChoicesContainer;
    public List<ColorChoice> colorChoices;

    [Header("Joined Lobby UI")]
    public Button joinedLobbyReadyButton;
    private LobbyMember player;
    private PlayerCard joinedPlayerCard;
    private bool isReady;


    private LobbyEntry selectedLobbyEntry = null;
    private LobbyData selectedLobbyData = null;
    private ColorChoice selectedColorChoice = null;
    private float fadeDuration = 0.2f;
    private float popInDuration = 0.2f;
    private Color readyColor;
    private Color notReadyColor;

    

    void Awake()
    {
        // get color choices
        colorChoices = new List<ColorChoice>(colorChoicesContainer.GetComponentsInChildren<ColorChoice>());

        foreach (var colorChoice in colorChoices)
        {
            Button btn = colorChoice.GetComponent<Button>();
            btn.onClick.AddListener(() => OnColorClicked(colorChoice));
        }

        selectedColorChoice = colorChoices[0];
        OnColorClicked(colorChoices[0]);

        // ui colors
        UnityEngine.ColorUtility.TryParseHtmlString("#16DA23", out readyColor); // green
        UnityEngine.ColorUtility.TryParseHtmlString("#9D9D9D", out notReadyColor); // gray

        // test player
        player = new LobbyMember();
        player.name = "You";
        player.colorChoice = selectedColorChoice;
        player.id = GenerateId();

        PreparePlayerCard();

        // add test lobby examples
        lobbies = new List<LobbyData>
        {
            new LobbyData
            {
                lobbyName = "Fun Lobby",
                id = 1,
                maxMembers = 4,
                host = new LobbyMember { id = GenerateId(), name = "Alice", colorChoice = colorChoices[0] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Bob", colorChoice = colorChoices[1] }
                }
            },
            new LobbyData
            {
                lobbyName = "Casual Lobby",
                id = 2,
                maxMembers = 4,
                host = new LobbyMember { id = GenerateId(), name = "Charlie", colorChoice = colorChoices[2] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Dana", colorChoice = colorChoices[3] }
                }
            },
            new LobbyData
            {
                lobbyName = "Pro Players",
                id = 3,
                maxMembers = 5,
                host = new LobbyMember { id = GenerateId(), name = "Eve", colorChoice = colorChoices[4] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Frank", colorChoice = colorChoices[5] },
                    new LobbyMember { id = GenerateId(), name = "Grace", colorChoice = colorChoices[1] }
                }
            },
            new LobbyData
            {
                lobbyName = "Chill Zone",
                id = 4,
                maxMembers = 4,
                host = new LobbyMember { id = GenerateId(), name = "Hank", colorChoice = colorChoices[6] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Ivy", colorChoice = colorChoices[3] }
                }
            },
            new LobbyData
            {
                lobbyName = "Adventure Squad",
                id = 5,
                maxMembers = 4,
                host = new LobbyMember { id = GenerateId(), name = "Jack", colorChoice = colorChoices[0] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Kim", colorChoice = colorChoices[2] },
                    new LobbyMember { id = GenerateId(), name = "Leo", colorChoice = colorChoices[1] }
                }
            },
            new LobbyData
            {
                lobbyName = "Night Owls",
                id = 6,
                maxMembers = 2,
                host = new LobbyMember { id = GenerateId(), name = "Mia", colorChoice = colorChoices[5] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Nina", colorChoice = colorChoices[2] }
                }
            },
            new LobbyData
            {
                lobbyName = "Speedsters",
                id = 7,
                maxMembers = 6,
                host = new LobbyMember { id = GenerateId(), name = "Oscar", colorChoice = colorChoices[3] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Pam", colorChoice = colorChoices[0] }
                }
            },
            new LobbyData
            {
                lobbyName = "Gamers United",
                id = 8,
                maxMembers = 4,
                host = new LobbyMember { id = GenerateId(), name = "Quinn", colorChoice = colorChoices[1] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Ray", colorChoice = colorChoices[2] },
                    new LobbyMember { id = GenerateId(), name = "Sara", colorChoice = colorChoices[4] }
                }
            },
            new LobbyData
            {
                lobbyName = "Elite Squad",
                id = 9,
                maxMembers = 3,
                host = new LobbyMember { id = GenerateId(), name = "Tom", colorChoice = colorChoices[2] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Uma", colorChoice = colorChoices[3] }
                }
            },
            new LobbyData
            {
                lobbyName = "Casual Gamers",
                id = 10,
                maxMembers = 4,
                host = new LobbyMember { id = GenerateId(), name = "Vera", colorChoice = colorChoices[0] },
                members = new List<LobbyMember>
                {
                    new LobbyMember { id = GenerateId(), name = "Will", colorChoice = colorChoices[2] },
                    new LobbyMember { id = GenerateId(), name = "Xander", colorChoice = colorChoices[1] }
                }
            }
        };
        foreach (var lobby in lobbies)
        {
            lobby.members.Insert(0, lobby.host);
        }
    }

    void Start()
    {
        MenuOpened();
    }

    public void MenuOpened()
    {
        StartCoroutine(WaitForFrame());

        CanvasGroup listCg = lobbyListContainer.GetComponent<CanvasGroup>();
        CanvasGroup selectedCg = selectedLobbyContainer.GetComponent<CanvasGroup>();

        listCg.alpha = 0f;
        selectedCg.alpha = 0f;

        StartCoroutine(MenuOpenRoutine(listCg, selectedCg));
    }

    private IEnumerator MenuOpenRoutine(CanvasGroup listCg, CanvasGroup selectedCg)
    {
        yield return StartCoroutine(RefreshListCoroutine(false));
        yield return StartCoroutine(FadeIn(listCg, selectedCg));
    }


    private IEnumerator WaitForFrame()
    {
        yield return null;
    }

    private string GenerateId()
    {
        return System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }


    public void RefreshList()
    {
        StartCoroutine(RefreshListCoroutine(true));
    }

    private IEnumerator RefreshListCoroutine(bool doFade = true)
    {
        CanvasGroup listCg = lobbyListContainer.GetComponent<CanvasGroup>();
        CanvasGroup selectedCg = selectedLobbyContainer.GetComponent<CanvasGroup>();

        // fade out
        if (doFade)
        {
            yield return StartCoroutine(FadeOut(listCg, selectedCg));
        }

        // clear old entries
        foreach (Transform child in lobbyListContainer)
        {
            Destroy(child.gameObject);
        }

        selectedLobbyEntry = null;
        selectedLobbyData = null;

        if (lobbies.Count == 0)
        {
            noLobbyText.gameObject.SetActive(true);
            noSelectedLobbyText.gameObject.SetActive(true);
            selectedLobbyContainer.gameObject.SetActive(false);
            yield break; // stop coroutine
        }
        else
        {
            noLobbyText.gameObject.SetActive(false);
            noSelectedLobbyText.gameObject.SetActive(false);
            selectedLobbyContainer.gameObject.SetActive(true);
        }

        PopulateLobbies();

        // select and display the selected lobbies
        yield return null;
        OnLobbyClicked(selectedLobbyEntry, selectedLobbyData, false);

        // fade in
        if (doFade)
        {
            yield return StartCoroutine(FadeIn(listCg, selectedCg));
        }
    }

    private void PopulateLobbies()
    {
        // create lobby entry for each lobby in the lobbies list
        foreach (var lobby in lobbies)
        {
            LobbyEntry entry = Instantiate(lobbyEntryPrefab, lobbyListContainer);

            entry.lobbyNameText.text = lobby.lobbyName;
            entry.hostText.text = "Host: " + lobby.host.name;
            entry.countText.text = lobby.members.Count + "/" + lobby.maxMembers;

            // check if full
            bool isFull = lobby.members.Count >= lobby.maxMembers;
            entry.SetFull(isFull);

            // add listener to button
            Button entryBtn = entry.GetComponent<Button>();
            entryBtn.onClick.AddListener(() => OnLobbyClicked(entry, lobby));

            // select first entry by default
            if (selectedLobbyEntry == null)
            {
                selectedLobbyEntry = entry;
                selectedLobbyData = lobby;
            }
        }
    }

    private void OnLobbyClicked(LobbyEntry entry, LobbyData lobby, bool doFade = true)
    {
        // unselect previous entry
        if (selectedLobbyEntry != null)
        {
            selectedLobbyEntry.Unselect();
        }

        // select new entry and display lobby
        selectedLobbyData = lobby;
        selectedLobbyEntry = entry;
        selectedLobbyEntry.Select();
        StartCoroutine(DisplayLobbyCoroutine(lobby, doFade));
    }

    private IEnumerator DisplayLobbyCoroutine(LobbyData lobby, bool doFade = true)
    {
        CanvasGroup cg = selectedLobbyContainer.GetComponent<CanvasGroup>();

        if (doFade)
        {
            yield return StartCoroutine(FadeOut(cg));
        }

        // clear old players
        foreach (Transform child in playersGrid)
        {
            Destroy(child.gameObject);
        }

        // set lobby name and host text
        selectedLobbyNameText.text = lobby.lobbyName;
        selectedHostNameText.text = "Host: " + lobby.host.name;

        // keep track of cards for pop in animations
        List<Transform> popInCards = new List<Transform>();

        // make player cards for each member
        foreach (var member in lobby.members)
        {
            GameObject playerCard = Instantiate(playerCardPrefab, playersGrid);
            PlayerCard card = playerCard.GetComponent<PlayerCard>();
            bool isHost = member.id == lobby.host.id;
            card.SetPlayerInfo(
                member.name, 
                member.colorChoice.colorName, 
                member.colorChoice.color, 
                isHost, 
                showReadyButton: false, 
                steamProfilePicture: null);

            popInCards.Add(playerCard.transform);
        }

        // fill waiting slots
        int remaining = lobby.maxMembers - lobby.members.Count;
        for (int i = 0; i < remaining; i++)
        {
            Instantiate(waitingCardPrefab, playersGrid);
        }
            
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectedLobbyContainer.GetComponent<RectTransform>());

        StartCoroutine(SequentialPopIn(popInCards));

        if (doFade)
        {
            yield return StartCoroutine(FadeIn(cg));
        }
    }

    public void JoinLobby()
    {
        StartCoroutine(JoinLobbyCoroutine());
    }

    private void PreparePlayerCard()
    {
        if (joinedPlayerCard != null) return; 
        GameObject playerCardObject = Instantiate(joinedPlayerCardPrefab, joinedPlayersGrid);
        joinedPlayerCard = playerCardObject.GetComponent<PlayerCard>();
    }


    private IEnumerator JoinLobbyCoroutine()
    {
        CanvasGroup cg = joinedLobbyContainer.GetComponent<CanvasGroup>();
        cg.alpha = 0;

        // clear old cards
        foreach (Transform child in joinedPlayersGrid) Destroy(child.gameObject);

        // ensure local player is in the lobby
        if (!selectedLobbyData.members.Contains(player))
        {
            selectedLobbyData.members.Add(player);
        }

        // update player color choice
        player.colorChoice = selectedColorChoice;

        // create cards for all members
        List<Transform> popInCards = new List<Transform>();
        foreach (var member in selectedLobbyData.members)
        {
            GameObject cardObj = Instantiate(joinedPlayerCardPrefab, joinedPlayersGrid);
            PlayerCard card = cardObj.GetComponent<PlayerCard>();
            bool isHost = member.id == selectedLobbyData.host.id;

            cardObj.SetActive(true);

            card.SetPlayerInfo(
                member.name,
                member.colorChoice.colorName,
                member.colorChoice.color,
                isHost,
                showReadyButton: true, 
                null
            );

            popInCards.Add(cardObj.transform);

            if (member == player)
            {
                joinedPlayerCard = card;

                joinedPlayerCard.SetReadyButtonVisible(true);
            }
        }


        // fill empty slots
        int remaining = selectedLobbyData.maxMembers - selectedLobbyData.members.Count;
        for (int i = 0; i < remaining; i++)
            Instantiate(inviteCardPrefab, joinedPlayersGrid);

        // setup main ready button
        joinedLobbyReadyButton.gameObject.SetActive(true);
        joinedLobbyReadyButton.onClick.RemoveAllListeners();
        joinedLobbyReadyButton.onClick.AddListener(joinedPlayerCard.ToggleReadyState);

        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(joinedLobbyContainer.GetComponent<RectTransform>());
        StartCoroutine(SequentialPopIn(popInCards));
        yield return StartCoroutine(FadeIn(cg));
    }

    /*
    private IEnumerator JoinLobbyCoroutine()
    {
        // get canvas group
        CanvasGroup cg = joinedLobbyContainer.GetComponent<CanvasGroup>();
        cg.alpha = 0;

        LeaveLobby();

        // clear old player cards
        foreach (Transform child in joinedPlayersGrid)
        {
            Destroy(child.gameObject);
        }

        // set lobby and host text
        selectedLobbyNameText.text = selectedLobbyData.lobbyName;
        selectedHostNameText.text = "Host: " + selectedLobbyData.host.name;

        List<Transform> popInCards = new List<Transform>();

        // create cards for other members 
        foreach (var member in selectedLobbyData.members)
        {
            GameObject tempCard = Instantiate(joinedPlayerCardPrefab, joinedPlayersGrid);
            PlayerCard card = tempCard.GetComponent<PlayerCard>();
            bool isHost = member.id == selectedLobbyData.host.id;

            card.SetPlayerInfo(
                member.name,
                member.colorChoice.colorName,
                member.colorChoice.color,
                isHost,
                showReadyButton: true,
                steamProfilePicture: null
            );

            popInCards.Add(tempCard.transform);
        }

        // add local player to list
        if (!selectedLobbyData.members.Contains(player))
        {
            selectedLobbyData.members.Add(player);
        }


        // prepare local player card and add last
        PreparePlayerCard();

        player.colorChoice = selectedColorChoice;
        bool isPlayerHost = player.id == selectedLobbyData.host.id;
        joinedPlayerCard.SetPlayerInfo(
            player.name,
            player.colorChoice.colorName,
            player.colorChoice.color,
            isPlayerHost,
            showReadyButton: true,
            steamProfilePicture: null
        );

        popInCards.Add(joinedPlayerCard.transform);

        // fill invite slots
        int remaining = selectedLobbyData.maxMembers - selectedLobbyData.members.Count;
        for (int i = 0; i < remaining; i++)
        {
            Instantiate(inviteCardPrefab, joinedPlayersGrid);
        }

        // set up ready button
        joinedLobbyReadyButton.onClick.RemoveAllListeners();
        joinedLobbyReadyButton.onClick.AddListener(joinedPlayerCard.ToggleReadyState);

        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(joinedLobbyContainer.GetComponent<RectTransform>());

        StartCoroutine(SequentialPopIn(popInCards));
        yield return StartCoroutine(FadeIn(cg));
    }
    */


    public void LeaveLobby()
    {
        selectedLobbyData.members.Remove(player);
    }

    public void OnReadyButtonPressed()
    {
        if (joinedPlayerCard == null) return;

        // toggle player card's state
        joinedPlayerCard.ToggleReadyState();

        // change main ready button visuals
        isReady = !isReady;
        TMP_Text readyText = joinedLobbyReadyButton.GetComponentInChildren<TMP_Text>();
        Image readyBg = joinedLobbyReadyButton.GetComponent<Image>();

        if (isReady)
        {
            readyText.text = "Ready";
            readyBg.color = readyColor;
        }
        else
        {
            readyText.text = "Not Ready";
            readyBg.color = notReadyColor;
        }
    }

    private void OnColorClicked(ColorChoice color)
    {
        // unselect previous entry
        if (selectedColorChoice != null)
        {
            selectedColorChoice.Unselect();
        }

        // select new entry and display lobby
        selectedColorChoice = color;
        selectedColorChoice.Select();
    }

    // animations
    private IEnumerator FadeIn(CanvasGroup cg)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
    }

    private IEnumerator FadeIn(CanvasGroup cg1, CanvasGroup cg2)
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

    private IEnumerator FadeOut(CanvasGroup cg1, CanvasGroup cg2)
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

    private IEnumerator PopIn(Transform target, float startScale = 0.8f)
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

    private IEnumerator SequentialPopIn(List<Transform> cards, float delayBetween = 0.05f)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            StartCoroutine(PopIn(cards[i]));
            yield return new WaitForSeconds(delayBetween); 
        }
    }

}
