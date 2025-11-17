using Mirror;
using UnityEngine;

public class AuthorityCheckerDebug : NetworkBehaviour
{
    [Server]
    public void CheckOwner(NetworkIdentity identity)
    {
        if (identity.connectionToClient != null)
        {
            Debug.Log($"Object {this.name} is owned by client connection ID: {identity.connectionToClient.connectionId}");
        }
        else
        {
            Debug.Log("Object has no client authority (server-owned).");
        }
    }
    [ServerCallback]
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            CheckOwner(GetComponent<NetworkIdentity>());
        }
    }
}
