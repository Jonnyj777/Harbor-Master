using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public uint appId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(this);
        try
        {
            Steamworks.SteamClient.Init(appId, true);
            Debug.Log("Steam is working");
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }


    }

    private void OnApplicationQuit()
    {
        try
        {
            Steamworks.SteamClient.Shutdown();
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
