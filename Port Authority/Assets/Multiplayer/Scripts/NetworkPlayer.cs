using UnityEngine;
using Mirror;
using System;

public class NetworkPlayer : NetworkBehaviour
{

    [SyncVar] public bool isReady;
    [SyncVar] public Vector3 lineColorData;
    [Command]
    public void CmdRequestAuthority(NetworkIdentity vehicle)
    {
        print("Authority: " + vehicle.connectionToClient);
        if (vehicle == null) return;
        if(vehicle.connectionToClient == connectionToClient) return;

        if (vehicle.connectionToClient != null && vehicle.connectionToClient != connectionToClient) return;

        //if(vehicle.connectionToClient != null)
        //{
        //    vehicle.RemoveClientAuthority();
        //}

        vehicle.AssignClientAuthority(connectionToClient);
        Debug.Log($"Authority granted to {connectionToClient.identity} for {vehicle.name}");
    }

    [Command]
    public void CmdRemoveAuthority(NetworkIdentity vehicle)
    {
        if (vehicle == null) return;

        if (vehicle.connectionToClient == connectionToClient)
        {
            vehicle.RemoveClientAuthority();
        }
        Debug.Log($"Authority removed for {vehicle.name}");
    }

    private void Ready(bool oldVal, bool newVal)
    {
        if(isServer)
        {
            this.isReady = newVal;
        }

        if (isClient)
        {
            //throw new NotImplementedException();
        }
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        this.Ready(this.isReady, !this.isReady);
    }

    public void UpdateReady()
    {
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }



}
