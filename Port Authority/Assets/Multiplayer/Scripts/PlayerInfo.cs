using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public class PlayerInfo
{
    public GameObject playerObj;
    public PlayerCard playerCardObj;

    private bool _isReady;

    public bool IsReady
    {
        get => _isReady;

        set
        {
            if(_isReady != value)
            {
                _isReady = value;
                onValueChanged.Invoke(_isReady);
            }
        }
    }

    public ToggleEvent onValueChanged = new ToggleEvent();
    public PlayerInfo(GameObject obj)
    {
        playerObj = obj;
        IsReady = false;

        //playerObj.GetComponentInChildren<Toggle>().onValueChanged.AddListener( (bool call) =>
        //{
        //    isReady = !isReady;
        //});
    }

    public PlayerInfo(PlayerCard obj)
    {
        playerCardObj = obj;
        IsReady = false;
    }

    public PlayerInfo()
    {
        playerObj = null;
        IsReady = false;
    }
}
