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

        if(!conn.isReady)
        {
            print("client is readied now");
            NetworkServer.SetClientReady(conn);
        }
    
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        
        //NetworkRoomPlayer oldRoomPlayer = conn.identity.GetComponent<NetworkRoomPlayer>();

        //if (oldRoomPlayer != null)
        //{
            //NetworkServer.Destroy(oldRoomPlayer.gameObject);
        //}
    }

    public override void OnRoomClientEnter()
    {
        base.OnRoomClientEnter();
    }

    public void MoveToGameplayScene()
    {
        foreach(var p in roomSlots)
        {
            Debug.Log($"RoomSlot: conn={p.connectionToClient.connectionId} ready={p.readyToBegin}");
        }

        if(NetworkServer.active && NetworkServer.connections.Count > 0)
        {
            ServerChangeScene(GameplayScene);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        if(numPlayers == 0)
        {
            //StopHost();
        }
    }

}
