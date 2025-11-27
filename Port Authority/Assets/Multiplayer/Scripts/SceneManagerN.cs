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
        //NetworkRoomManager.singleton.ServerChangeScene(MainMenuScene);
        
        if(NetworkClient.activeHost)
        {
            //NetworkRoomManager.singleton.StopHost();
        }
        else if(NetworkClient.isConnected)
        {
            //NetworkRoomManager.singleton.StopClient();
        }

        SteamLobbyManagerUITest.instance.RemoveCallbacks();
        

        SteamLobbyManagerUITest.Lobby.Leave();
        /*
        SteamLobbyManagerUITest inst = SteamLobbyManagerUITest.instance;

        if(inst != null)
        {
            Destroy(inst.gameObject);
        }

        if (NetworkLobby.singleton != null)
        {
            Destroy(NetworkLobby.singleton.gameObject);
        }

        Steamworks.SteamClient.Shutdown();
        */
        SceneManager.LoadScene(MainMenuScene);
   
    }
}
