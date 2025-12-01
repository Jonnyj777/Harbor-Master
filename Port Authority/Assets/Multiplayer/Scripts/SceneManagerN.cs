using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerN : MonoBehaviour
{

    public static bool isGameStarted = false;

    [Scene]
    public string GameplayScene;

    [Scene]
    public string MainMenuScene;

    [Server]
    public void RestartScene()
    {
        NetworkRoomManager.singleton.ServerChangeScene(GameplayScene);
    }

    public void Start()
    {
        isGameStarted = true;
    }


    public void ChangeToMainMenuScene()
    {
        isGameStarted = false;
        //SteamLobbyManagerUITest.instance.RemoveCallbacks();
        SteamLobbyManagerUITest.instance.LeaveLobby();

        if (NetworkServer.active)
        {
            Debug.LogError("active host is disconnecting to main menu");
            //NetworkServer.DisconnectAll();
            NetworkManager.singleton.StopHost();
            SceneManager.LoadScene(MainMenuScene);
            return;
        }
        
        if(NetworkClient.isConnected)
        {
            Debug.LogError("client is disconnecting to main menu");
            //NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(MainMenuScene);
            return;
        }

        SceneManager.LoadScene(MainMenuScene);

    }
}
