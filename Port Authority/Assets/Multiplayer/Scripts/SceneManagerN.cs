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

        if (NetworkClient.activeHost)
        {
            NetworkManager.singleton.StopHost();
            SceneManager.LoadScene(MainMenuScene);
            return;
        }
        
        if(NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(MainMenuScene);
            return;
        }

        SceneManager.LoadScene(MainMenuScene);

    }
}
