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
        if (vehicle == null) return;
        if(vehicle.connectionToClient == connectionToClient) return;

        if(vehicle.connectionToClient != null)
        {
            vehicle.RemoveClientAuthority();
        }

        vehicle.AssignClientAuthority(connectionToClient);
        Debug.Log($"Authority granted to {connectionToClient.identity} for {vehicle.name}");
    }
}
