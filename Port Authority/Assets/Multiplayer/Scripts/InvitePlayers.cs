using Mirror;
using Steamworks;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvitePlayers : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    public void OpenInviteOverlay()
    {
        print("Overlay: " + SteamLobbyManager.isLobbySet);
        if (SteamLobbyManager.isLobbySet)
        {
            SteamFriends.OpenGameInviteOverlay(SteamLobbyManager.Lobby.Id);
            text.text = "Overlay Sent";
            
        }
    }

}
