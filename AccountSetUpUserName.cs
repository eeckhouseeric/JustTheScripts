using PlayFab;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class AccountSetUpUserName : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField userName;
    [SerializeField] private TMP_InputField firstName;
    [SerializeField] private TMP_InputField lastName;

    [Header("Warnings")]
    [SerializeField] private TextMeshProUGUI emailWarning;
    [SerializeField] private TextMeshProUGUI userNameWarning;

    private bool isValid = false;

    /// <summary>
    /// Validates the email and username input fields.
    /// <summary>

    public void ValidateInput()
    {
        isValid = true;

        //Email Validation
        if (string.IsNullOrEmpty(email.text) || !Regex.IsMatch(email.text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {

            emailWarning.text = "Please enter a valid email address!";
            isValid = false;

        }
        else
        { 
            emailWarning.text = "";

        }

        //Username Validation
        if (string.IsNullOrEmpty(userName.text) || userName.text.Length < 3)
        { 
            userNameWarning.text = "Username must be at least 3 characters!";
            isValid = false;


        }
        else
        { 
            userNameWarning.text = "";
        }



    }

    /// <summary>
    /// Attempts to register with PlayFab to check availibility.
    /// <summary>
    public void CheckAvailability(string password)
    {
        ValidateInput();
        if (!isValid) return;

        var request = new PlayFab.ClientModels.RegisterPlayFabUserRequest
        {
            Email = email.text,
            Username = userName.text,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterError);    
    }

    private void OnRegisterSuccess(PlayFab.ClientModels.RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration successful! PlayFabId: " + result.PlayFabId);
        emailWarning.text = "";
        userNameWarning.text = "";
        isValid = true;
        // You can now move to the next prefab (birthday/location)

    }

    private void OnRegisterError(PlayFabError error)
    {
        Debug.LogError("Error during registration: " + error.GenerateErrorReport());
        isValid = false;

        switch (error.Error)
        { 
            case PlayFabErrorCode.UsernameNotAvailable:
                userNameWarning.text = "Username is already taken!";
                break;

            case PlayFabErrorCode.AccountAlreadyExists:
                emailWarning.text = "Email address is Already Exist!";
                break;

            case PlayFabErrorCode.InvalidEmailAddress:
                emailWarning.text = "Email address is invalid!";
                break;
            case PlayFabErrorCode.EmailAddressNotAvailable:
                emailWarning.text = "Email is already in use!";
                break;
            
            
            
            default:
                emailWarning.text = "Error: " + error.ErrorMessage;
                break;


        }
    }

    public void OnNextButtonPressed()
    {
        ValidateInput();
        if (isValid)
        {
            AccountCreationFlow.Instance.SetUserNameData(
                GetEmail(), GetUserName(), GetFirstName(), GetLastName()
                );
            AccountCreationFlow.Instance.GoToNextStep(this);
        }
    }




    public bool IsInputValid() => isValid;
    public string GetEmail() => email.text;
    public string GetUserName() => userName.text;
    public string GetFirstName() => firstName.text;
    public string GetLastName() => lastName.text;

}
