using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerN : MonoBehaviour
{

    [Scene]
    public string GameplayScene;

    [Scene]
    public string MainMenuScene;

    [Server]
    public void RestartScene()
    {
        NetworkRoomManager.singleton.ServerChangeScene(GameplayScene);
    }


    public void ChangeToMainMenuScene()
    {
        //SteamLobbyManagerUITest.instance.RemoveCallbacks();
        SteamLobbyManagerUITest.Lobby.Leave();

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
