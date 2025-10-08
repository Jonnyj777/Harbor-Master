using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo
{
    public GameObject playerObj;

    public bool isReady;
    public PlayerInfo(GameObject obj)
    {
        playerObj = obj;
        isReady = false;

        playerObj.GetComponentInChildren<Toggle>().onValueChanged.AddListener( (bool call) =>
        {
            isReady = !isReady;
        });
    }
}
