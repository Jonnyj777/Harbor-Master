
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReferenceGrab : MonoBehaviour
{
    public Button startButton;

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

    [Header("Create Lobby UI")]
    public TMP_InputField lobbyNameInput;
    public TMP_InputField lobbySizeInput;

    public Button hostButton;
    public Button refreshButton;
    public Button createLobbyButton;
    public Button leaveButton;

    private void Start()
    {
        print("initialize");
        SteamLobbyManagerUITest.instance.InitializeMenu(this);
    }
}
