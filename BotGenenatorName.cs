using UnityEngine;

public class BotGeneratorName : MonoBehaviour
{
    private static string[] prefixes = { "Byte", "Null", "Ping", "Stack", "Ghost" };
    private static string[] suffixes = { "Crusher", "Queen", "Sniper","Jack","Lord" };

    public static string generateName()
    {
        string name = prefixes[Random.Range(0, prefixes.Length)] + suffixes[Random.Range(0,suffixes.Length)];
       // Debug.Log($"[BotGeneratorName] Generated bot name: {name}");
        return name;
    }
}
