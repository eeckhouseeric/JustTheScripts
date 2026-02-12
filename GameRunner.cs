using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameRunner : SimulationBehaviour
{
    public static GameRunner Instance;

    private Dictionary<PlayerRef, int> playerTeams = new();
    private Dictionary<int, List<PlayerRef>> teamMembers = new();

    [Networked] public int MaxTeams { get; private set; } = 2;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("GameRunner spawned as state authority.");
        }
    }

    public void AssignTeam(PlayerRef player)
    {
        int teamID = player.RawEncoded % MaxTeams;
        playerTeams[player] = teamID;

        if (!teamMembers.ContainsKey(teamID))
            teamMembers[teamID] = new();

        teamMembers[teamID].Add(player);
    }

    public int GetTeamID(PlayerRef player)
    {
        return playerTeams.TryGetValue(player, out int teamID) ? teamID : -1;
    }

    public int GetPlayerIndex(PlayerRef player)
    {
        int teamID = GetTeamID(player);
        return teamMembers.ContainsKey(teamID)
            ? teamMembers[teamID].IndexOf(player)
            : -1;
    }


}
