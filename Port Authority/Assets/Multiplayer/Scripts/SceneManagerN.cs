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
        SteamLobbyManagerUITest.instance.LeaveLobby();
        SceneManager.LoadScene(MainMenuScene);

    }
}
