using Unity.VisualScripting;
using UnityEngine;

public class GameSpawnerManger : MonoBehaviour
{

    public static GameSpawnerManger instance { get; private set; }
    [Header("Team Spawn Points")]
    public Transform[] redSpawns;
    public Transform[] blueSpawns;

    private void Awake()
    {
        instance = this;
    }

    public Transform GetSpawnPointForTeam(int teamID)
    {

        Transform[] arr = teamID == 0 ? redSpawns : blueSpawns;


        if (arr != null && arr.Length > 0)
        {
            int i = Random.Range(0, arr.Length);
            return arr[i];
        }

        Debug.LogWarning("[GameSpawnerManager] No spawn points found for team " + teamID);
        return null;
    }
}



