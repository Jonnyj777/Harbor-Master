using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NamedColor
{
    public string name;
    public string hex;
    public Color color; // make this a normal public field, not a property
}

[System.Serializable]
public class LobbyMember
{
    public string name;
    public string colorName; // e.g., "Red"

    // optional static reference to manager (set in Start)
    public static LobbyListManager LobbyListManagerInstance;
}

[System.Serializable]
public class LobbyData
{
    public string lobbyName;
    public int id;
    public int maxMembers;

    public LobbyMember host;             // host is now a LobbyMember
    public List<LobbyMember> members;    // other members (including host optionally)
}

