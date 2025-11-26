using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerN : MonoBehaviour
{

    [Server]
    public void RestartScene()
    {
        NetworkRoomManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
    }

    [Server]

    public void ChangeToMainMenuScene()
    {
        NetworkRoomManager.singleton.ServerChangeScene("MultiplayerMenuConnecting2");
    }
}
