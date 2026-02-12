using System.Collections.Generic;
using UnityEngine;

public class TeamSpawnPoint : MonoBehaviour
{
    public enum Team { Red = 0, Blue = 1 }
    public Transform spawnPointTransform;
    public GameObject cube;
    [Header("Team Assignment")]
    public Team team;

    [Header("Spawn Points for Each Team")]
    public List<List<Transform>> spawnPoints = new();

    /// <summary>
    /// Returns the spawn point for a given team and player index.
    /// </summary>
    /// 
    private void Start()
    {
        cube.SetActive(false);
    }

    public Transform GetSpawnPoint(Team teamEnum, int playerIndex)
    {
        int teamID = (int)teamEnum;

        if (teamID >= 0 && teamID < spawnPoints.Count &&
            playerIndex >= 0 && playerIndex < spawnPoints[teamID].Count)
        {
            return spawnPoints[teamID][playerIndex];
        }

        Debug.LogWarning($"Invalid team: {teamEnum} or player index: {playerIndex} in GetSpawnPoint.");
        return null;
    }
}



  
