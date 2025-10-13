using UnityEngine;
using Mirror;

public class NetworkAuthorizer : NetworkBehaviour
{
    private void Start()
    {
        print("Start function for authorizer called");
    }
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
