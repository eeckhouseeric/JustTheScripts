using UnityEngine;
using Fusion;
using Fusion.Analyzer;
public class PlaneHealth : NetworkBehaviour
{
    //Networked health with Fusion 2.0.6 change callback
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int Health { get; set; }

    // Last timestamp of damage applied (anti-replay)
    [Networked] public int LastDamageTimestamp { get; set; }

    //Server-side rate limiting (anti-rapid-fire hacks)
    [Networked] public int LastDamageSourceTimestamp { get; set; }

    public int MaxHealth = 100;

    // UI event for local binding
    public event System.Action<int, int> OnHealthUpdated;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            Health = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (!Object.HasStateAuthority)
            return;
        Health = Mathf.Max(Health - amount, 0);
    }

    public void OnHealthChanged()
    {
        NotifyUI();
    }

    public void NotifyUI()
    {
        OnHealthUpdated?.Invoke(Health, MaxHealth);
    }

    // Secure server-only damage entry point
    public void ServerApplyDamage(int amount, int timestamp)
    {

        if (!Object.HasStateAuthority)
            return;// Clients cannot apply damage

        //Reject impossible values
        if (amount <= 0 || amount > 500)
        { 
            Debug.LogWarning($"[SERCURITY] Invalid damage amount {amount} from {Object.InputAuthority}");
            return;
        }

        // reject replay attacks (example: 100ms cooldown)
        if (timestamp - LastDamageTimestamp < 100)
        {
            Debug.LogWarning($"[SECURITY] Replay attack detected from {Object.InputAuthority}");
            return;
        }

        LastDamageTimestamp = timestamp;
        LastDamageSourceTimestamp = timestamp;
        Health = Mathf.Max(Health - amount, 0);


    }


}
