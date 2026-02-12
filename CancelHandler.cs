using UnityEngine;

public class CancelHandler : MonoBehaviour
{
    private GameObject loginInstance;

    // Called by PlayFabAuth when spawning AccountCreation
    public void AssignLoginReference(GameObject login)
    {
        loginInstance = login;
    }

    public void OnCancelPressed()
    {
        // Destroy the AccountCreation prefab root
        Destroy(transform.root.gameObject);

        // Reactivate the hidden login
        if (loginInstance != null)
        {
            loginInstance.SetActive(true);
        }
        else
        {
            Debug.LogError("[CancelHandler] Login reference not assigned.");
        }
    }
}