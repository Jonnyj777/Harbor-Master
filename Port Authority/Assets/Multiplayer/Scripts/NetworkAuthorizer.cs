using UnityEngine;
using Mirror;

public class NetworkAuthorizer : NetworkBehaviour
{
    [Command]
    public void CmdRequestAuthority(NetworkIdentity vehicle)
    {
        print("request 1: " + vehicle.connectionToClient);
        if(vehicle.connectionToClient == null)
        {
            vehicle.AssignClientAuthority(connectionToClient);
            print("request 2 : " + vehicle.connectionToClient);
            print("isOwned: " + vehicle.isOwned);
        }
    }
}
