using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public TeamSpawnPoint[] allSpawns;

    public Transform GetSpawnPoint(TeamSpawnPoint.Team team, int playerIndex)
    {
        // Filter spawn points by team
        var teamSpawns = allSpawns.Where(spawn => spawn.team == team).ToArray();

        // Handle missing spawn points gracefully
        if (teamSpawns.Length == 0)
        {
            Debug.LogWarning($"No spawn points found for team: {team}");
            return null;
        }

        // Wrap index to avoid out-of-range errors
        int wrappedIndex = Mathf.Abs(playerIndex % teamSpawns.Length);
        return teamSpawns[wrappedIndex].transform;
    }
}
