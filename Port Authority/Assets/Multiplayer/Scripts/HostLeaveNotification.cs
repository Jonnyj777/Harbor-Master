using UnityEngine;
using Mirror;

public class HostLeaveNotification : MonoBehaviour
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
    public void HostLeft()
    {
        ShowHostLeftNotification();
    }

    public void ShowHostLeftNotification()
    {
        print("client receive notifcation");
        hostLeaveBox.gameObject.SetActive(true);
    }
}
