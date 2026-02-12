using UnityEngine;

public class AccountCreationFlow : MonoBehaviour
{
    public static AccountCreationFlow Instance { get; private set; }

    [Header("Step Prefabs (assign in order: Username + Password + Birthday/Location + Terms)")]
    [SerializeField] private GameObject[] stepPrefabs;

    private GameObject currentStepInstance;
    private GameObject loginPanel;
    private PlayFabAuth auth;
    private int currentStep = 0;

    [System.Serializable]
    public class AccountData
    {
        public string Email;
        public string UserName;
        public string FirstName;
        public string LastName;
        public string Password;
        public string Birthday;
        public string Location;
        public bool AcceptedTerms;
    }

    public AccountData CollectData { get; private set; } = new AccountData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ShowStep(0); // start with Username step
    }
    /// <summary>
    /// Instantiates the step prefab at the given index, destroying the old one.
    /// </summary>
    
    public void AssignLoginReference(GameObject login)
    {
        loginPanel = login;
    }

    public void AssignAuthReference(PlayFabAuth playFabAuth)
    {
        auth = playFabAuth;
    }


    /// <summary>
    /// Instantiates the step prefab at the given index, destroying the old one.
    /// </summary>
    private void ShowStep(int index)
    {
        // Destroy old step if it exists
        if (currentStepInstance != null)
            Destroy(currentStepInstance);

        if (index < stepPrefabs.Length)
        {
            currentStepInstance = Instantiate(stepPrefabs[index], transform);
            currentStep = index;
            Debug.Log($"[AccountCreationFlow] Showing step {index}: {stepPrefabs[index].name}");




            var cancelHandler = currentStepInstance.GetComponentInChildren<CancelHandler>(true);
            if (cancelHandler != null)
            {
                cancelHandler.AssignLoginReference(loginPanel);
                Debug.Log("[AccountCreationFlow] Assigned login reference to CancelHandler." + stepPrefabs[index].name);
            }
            else
            {
                Debug.Log("[AccountCreationFlow] No more steps. Flow complete." + stepPrefabs[index].name);

            }
        }

        else
        {
            OnFlowComplete();
        }
    }




    /// <summary>
    /// Called by each step script when validation passes.
    /// </summary>
    public void GoToNextStep(MonoBehaviour caller)
    {
        Debug.Log($"[AccountCreationFlow] Advancing from {currentStep} to {currentStep + 1}, total prefabs: {stepPrefabs.Length}");
        ShowStep(currentStep + 1);
    }

    /// <summary>
    /// Reset the flow back to step 0.
    /// </summary>
    public void ResetFlow()
    {
        ShowStep(0);
        Debug.Log("[AccountCreationFlow] Flow reset to step 0.");
    }

    // ---------------------------
    // Data setters
    // ---------------------------
    public void SetUserNameData(string email, string username, string firstName, string lastName)
    {
        CollectData.Email = email;
        CollectData.UserName = username;
        CollectData.FirstName = firstName;
        CollectData.LastName = lastName;
    }

    public void SetPasswordData(string password) => CollectData.Password = password;

    public void SetBirthdayData(string birthday, string location)
    {
        CollectData.Birthday = birthday;
        CollectData.Location = location;
    }

    public void SetTermsAccepted(bool accepted) => CollectData.AcceptedTerms = accepted;

    // ---------------------------
    // Final step
    // ---------------------------
    private void OnFlowComplete()
    {
        Debug.Log("[AccountCreationFlow] Flow complete, attempting PlayFab registration");

        if (!CollectData.AcceptedTerms)
        {
            Debug.LogError("[AccountCreationFlow] User must accept terms to create an account.");
            return;
        }

        if (auth == null)
        {
            Debug.LogError("[AccountCreationFlow] PlayFabAuth component not found in scene.");
            return;
        }
        // debug dump of collected data
        Debug.Log($"[AccountCreationFlow] Email={CollectData.Email}" +
            $", Username={CollectData.UserName}," +
            $" Password(len)={(CollectData.Password?.Length ?? 0)}," +
            $" Birthday={CollectData.Birthday}," +
            $" Location={CollectData.Location}");

        // Push collected data into PlayFabAuth
        auth.emailInput.text = CollectData.Email;
        auth.passwordInput.text = CollectData.Password;

        // Trigger registration
        auth.OnRegisterButtonn();
    }
}
