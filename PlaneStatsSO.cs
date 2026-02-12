using UnityEngine;



[CreateAssetMenu(fileName = "PlaneStats", menuName = "Plane/Stats")]

public class PlaneStatsSO : ScriptableObject
{
    public string planeName;
    public int maxHealth;
    public int speed;
    public int armor;
    public float thrustSpeed;
    public float pitchSpeed;
    public float yawSpeed;
    public float rollSpeed;
}
