using Fusion;
using UnityEngine;

public class HealthBinderUi : MonoBehaviour
{
private void Start()
    {

        var runner = FindObjectOfType<NetworkRunner>();
        var obj = runner.GetPlayerObject(runner.LocalPlayer);

        if(obj.TryGetComponent(out PlaneHealth health))
        {

            GetComponentInChildren<HealthUIController>().Bind(health);
        }

    }
}
