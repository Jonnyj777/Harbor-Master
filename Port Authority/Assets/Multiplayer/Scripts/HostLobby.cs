using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HostLobby : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.singleton.StartHost();
            print(NetworkManager.singleton.networkAddress);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
