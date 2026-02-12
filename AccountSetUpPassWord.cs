using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountSetUpPassWord : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    public TextMeshProUGUI warningMessage;

    private bool passwordMatch = false;
    private int characterCount;




    private void Start()
    {
        if(warningMessage != null)
        {
            warningMessage.text = "";
        }
    }


    private void ValidatePassword()
    {
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;


        if (password.Length <= 12)
        {
            passwordMatch = false;
            warningMessage.text = "Password must be at least 12 characters!";
            Debug.Log("Password must be at least 12 characters!");
            return;
        }


        if(!System.Text.RegularExpressions.Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
        {
            warningMessage.text = "Password must contain at least one special character!";
            Debug.Log("Password must contain at least one special character!");
            passwordMatch = false;
            return;

        }


        if (password != confirmPassword)
        {
            warningMessage.text = "Passwords do not match!";
            Debug.Log("Passwords do not match!");
            passwordMatch = false;
            return;
        }

        if (password == confirmPassword)
        {
            passwordMatch = true;
            Debug.Log("Passwords match!");
            return;
        }
        // Pass all checks
        warningMessage.text = "Password Matches";
        passwordMatch = true;

    }

    public void OnNextButtonPressed()
    {
        ValidatePassword();
        if (passwordMatch)
        {
           AccountCreationFlow.Instance.SetPasswordData(passwordInput.text);
            AccountCreationFlow.Instance.GoToNextStep(this);
        }
    }

    public string GetPassword() => passwordMatch ? passwordInput.text : null;
    public bool IsPasswordValid() => passwordMatch;



}
