using UnityEngine;
using Steamworks;
using Mirror;
using Mirror.FizzySteam;

public class SteamManager : MonoBehaviour
{
    public uint appId;

    public static SteamManager Singleton { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            //DontDestroyOnLoad(gameObject);
            try
            {
                //SteamClient.Init(appId, true);
                Debug.Log("Steam is working");
            }
            catch(System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        else
        {
            Destroy(gameObject);
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
