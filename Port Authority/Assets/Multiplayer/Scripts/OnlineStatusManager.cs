using UnityEngine;

public class OnlineStatusManager : MonoBehaviour
{
    public static bool isOnline = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
