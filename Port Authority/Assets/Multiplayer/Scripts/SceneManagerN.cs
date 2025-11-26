using Mirror;
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

    [Server]

    public void ChangeToMainMenuScene()
    {
        //NetworkRoomManager.singleton.ServerChangeScene(MainMenuScene);
        NetworkRoomManager.singleton.StopClient();
    }
}
