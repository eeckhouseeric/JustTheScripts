using UnityEngine;
using UnityEngine.UI;



public class AccountSetupTermsAndConditions : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Toggle acceptToggle;
    [SerializeField] private Button createAccountButton;

    private bool hasReachedBottom = false;

    void Start()
    {
        // Safety checks
        if (scrollRect == null) Debug.LogError("ScrollRect not assigned in inspector!");
        if (acceptToggle == null) Debug.LogError("AcceptToggle not assigned in inspector!");
        if (createAccountButton == null) Debug.LogError("CreateAccountButton not assigned in inspector!");

        // Default state
        acceptToggle.interactable = false;
        createAccountButton.interactable = false;

        // If content fits without scrolling, enable toggle immediately
        if (scrollRect != null && scrollRect.content != null && scrollRect.viewport != null)
        {
            bool contentFits = scrollRect.content.rect.height <= scrollRect.viewport.rect.height;
            if (contentFits)
            {
                hasReachedBottom = true;
                acceptToggle.interactable = true;
                Debug.Log("Terms fit without scrolling. Toggle enabled immediately.");
            }
            else
            {
                scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            }
        }

        // Listen for toggle changes
        if (acceptToggle != null)
            acceptToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnScrollValueChanged(Vector2 position)
    {
        // Check if scrolled to bottom
        if (!hasReachedBottom && scrollRect.verticalNormalizedPosition <= 0.01f)
        { 
            hasReachedBottom = true;
            acceptToggle.interactable = true; // Enable toggle
            Debug.Log("User reached the bottom of Terms and Conditions. Toggle enable.");
        }
    }       

    private void OnToggleChanged(bool isOn)
    {
        // Only enable create account button if toggle is on
        createAccountButton.interactable = isOn;
    }

    public void OnCreateAccountPressed()
    {
        Debug.Log($"CreateAccount pressed. Toggle state={acceptToggle.isOn}");

        if (acceptToggle.isOn)
        {
            if (AccountCreationFlow.Instance != null)
            {
                AccountCreationFlow.Instance.SetTermsAccepted(true);
                AccountCreationFlow.Instance.GoToNextStep(this);
                Debug.Log("Terms accepted. Proceeding to next step.");
            }
            else
            {
                Debug.LogError("AccountCreationFlow.Instance is null! Make sure the manager exists in the scene.");
            }
        }
        else
        {
            Debug.Log("Please accept the Terms and Conditions to create an account.");
        }
    }

}
