using UnityEngine;
using UnityEngine.UI;

public class OfflineNotification : MonoBehaviour
{
    public Button multiplayerButton;

    void Start()
    {
        multiplayerButton.interactable = true;
    }
}
