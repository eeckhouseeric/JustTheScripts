using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class PlayFabAuth : MonoBehaviour
{
    [Header("UI Login Reference")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI messageText;

    [SerializeField] private GameObject accountCreationPrefab;
    [SerializeField] private GameObject forgotPasswordPrefab;
    [SerializeField] private Transform uiParent;


    void Start()
    {
       emailInput.onSelect.AddListener(OpenSystemKeyboard);
       passwordInput.onSelect.AddListener(OpenSystemKeyboard);
    }





    /// <summary>
    /// Console KeyBoard
    /// </summary>

    private void OpenSystemKeyboard(string _)
    {
        #if UNITY_XBOXONE || UNITY_GAMECORE || UNITY_PS4 || UNITY_PS5 || UNITY_SWITCH
            TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        #endif
    }


    /// <summary>
    /// Registration
    /// </summary>


    public void OnRegisterButtonn()
    {
        string email = emailInput.text;
        string paswword = passwordInput.text;


        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = paswword,
            Username = email.Split('@')[0], // or CollectData.UserName if you want
            RequireBothUsernameAndEmail = true

        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);

    }


    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Registration successful!";
        Debug.Log("Registration successful! session ticket: " + result.SessionTicket);

        PlayerSession.PlayFabID = result.PlayFabId;
        PlayerSession.SessionTicket = result.SessionTicket;
        PlayerSession.Username = result.Username ?? result.PlayFabId;
        PlayerPrefs.SetString("Playername", PlayerSession.Username);


        // Flip button to "Start"
        var startMenu = FindObjectOfType<StartMenuPrototype>();
        if (startMenu != null)
            startMenu.SetButtonToStart();

        // Return to StartMenu scene so button now says "Start"
        SceneManager.LoadScene("StartMenu");
    }

    private void OnLoginSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Login successful";
        Debug.Log("Login successful! session ticket: " + result.SessionTicket);
        
        PlayerSession.PlayFabID = result.PlayFabId;
        PlayerSession.SessionTicket = result.SessionTicket;

        PlayerSession.Username = result.Username ?? result.PlayFabId;




        SceneManager.LoadScene("lobby");
        // TODO: Pass SessionTicket or PlayFabId into Fusion multiplayer auth

    }

    public void OnCreationButton()
    {
        // Hide login
        gameObject.SetActive(false);

        // Show account creation flow
        var accountUI = Instantiate(accountCreationPrefab);
        accountUI.SetActive(true);

        // Pass login reference into CancelHandler
        var cancelHandler = accountUI.GetComponentInChildren<CancelHandler>();
        if (cancelHandler != null)
        {
            cancelHandler.AssignLoginReference(gameObject);
        }

        // Pass login reference into AccountCreationFlow
        var flow = accountUI.GetComponent<AccountCreationFlow>();
        if (flow != null)
        {
            flow.AssignLoginReference(gameObject);
            flow.AssignAuthReference(this);
        }

        else
        {
            Debug.LogError("[PlayFabAuth] AccountCreationFlow component not found in Account Creation UI.");
        }

    }


    // ---------------------------
    // Login
    // ---------------------------
    public void OnLoginButton()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
      
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Email and password are required.";
            return;
        }



        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            TitleId = PlayFabSettings.TitleId, // make sure this is set in your PlayFab settings
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        messageText.text = "Login successful!";
        Debug.Log("Login successful! Session ticket: " + result.SessionTicket);
       
        PlayerSession.PlayFabID = result.PlayFabId;
        PlayerSession.SessionTicket = result.SessionTicket;
        PlayerSession.Username = result.InfoResultPayload.AccountInfo.Username ?? result.PlayFabId;

        // Set PlayerPrefs so fusion usese correct username
        PlayerPrefs.SetString("Playername", PlayerSession.Username);

        // Flip button to "Play"
        var startMenu = FindObjectOfType<StartMenuPrototype>(true);
        if (startMenu != null)
            startMenu.SetButtonToStart();

        // Hide login UI and show StartMenu again
        gameObject.SetActive(false);
        if (startMenu != null)
        {
            startMenu.gameObject.SetActive(true);
            startMenu.SetButtonToPlay();
        }
        gameObject.SetActive(false);// Hide login UI
    }

    // ---------------------------
    // Password Recovery
    // ---------------------------
    public void OnForgotPasswordButton()
    {
        Debug.Log("Fogot Password button pressed");

        if(forgotPasswordPrefab == null)
        {
            Debug.LogError("Forgot Password Prefab is not assigned in the inspector.");
        }
        if(uiParent == null)
        {
            Debug.LogError("UI Parent Transform is not assigned in the inspector.");
        }


        //Hide login UI
        gameObject.SetActive(false);

        //Show forgot password UI
        var forgotPasswordUI = Instantiate(forgotPasswordPrefab);
        forgotPasswordUI.SetActive(true);
        Debug.Log("Forgot Password UI instantiated"+ forgotPasswordUI.name);



        //wire up CancelHandler
        var cancelHandler = forgotPasswordUI.GetComponentInChildren<CancelHandler>();
        if (cancelHandler != null)
        {
            cancelHandler.AssignLoginReference(gameObject);
        }
        else
        {
            Debug.LogError("[PlayFabAuth]CancelHandler component not found in Forgot Password UI.");
        }
    }



    /// <summary>
    /// Error Handling
    /// </summary>

    private void OnError(PlayFabError error)
    {
        //Show UI  user-friendly message
        messageText.text = "Error: " + error.ErrorMessage;

        //Log full error to the console as error message
        Debug.LogError("PlayFab Error: " + error.GenerateErrorReport());
    }


}
