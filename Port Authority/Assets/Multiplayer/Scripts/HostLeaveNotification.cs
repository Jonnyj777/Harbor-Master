using UnityEngine;
using Mirror;

public class HostLeaveNotification : NetworkBehaviour
{
    [SerializeField]
    private RectTransform hostLeaveBox;

    public static HostLeaveNotification instance; 

    public void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckHostLeft()
    {
        print("check host left");
        if(isServer)
        {
            print("is server");
            HostLeft();
        }
    }

    [Server]
    private void HostLeft()
    {
        RpcShowHostLeftNotification();
    }

    [ClientRpc]
    public void RpcShowHostLeftNotification()
    {
        print("client receive notifcation");
        hostLeaveBox.gameObject.SetActive(true);
    }
}
