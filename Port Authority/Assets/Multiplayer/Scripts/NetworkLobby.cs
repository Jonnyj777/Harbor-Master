using Mirror;
using TMPro;
using UnityEngine;

public class NetworkLobby : NetworkRoomManager
{

    [SerializeField]
    private Canvas lobbyCanvas;

    [SerializeField]
    private TextMeshProUGUI playerList;

    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn) {
        foreach(PendingPlayer player in pendingPlayers)
        {
            playerList.text += "\n" + player;

        }
    
    }

    public override void OnRoomClientEnter()
    {
        base.OnRoomClientEnter();
    }

}
