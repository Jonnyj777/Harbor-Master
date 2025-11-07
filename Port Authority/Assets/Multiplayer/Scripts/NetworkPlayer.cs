using UnityEngine;
using Mirror;
using System;

public class NetworkPlayer : NetworkBehaviour
{

    [SyncVar] public bool isReady;
    [SyncVar] public Vector3 lineColorData;
    private void Start()
    {
        lineColorData = new Vector3(0f, 1f, 0f);
        if(isServer)
        {
            lineColorData = new Vector3(1f, 0f, 0f);
        }
    }
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
