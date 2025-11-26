using UnityEngine;
using Mirror;

public class LoadToSceneN : MonoBehaviour
{
    [Scene]
    public string GameplayScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkRoomManager.singleton.ServerChangeScene(GameplayScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
