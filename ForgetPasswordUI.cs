using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;


public class ForgetPasswordUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button resetPasswordButton;
    [SerializeField] private Button cancelButton;

    public void OnSendRecoveryEmail()
    {
        if (string.IsNullOrEmpty(emailInput.text))
        {
            feedbackText.text = "Please enter your email address.";
            return;
        }

        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInput.text.Trim(),
            TitleId = PlayFabSettings.TitleId
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request,
            result => feedbackText.text = " Recovery email sent! Check your inbox",
            error =>
            {
                feedbackText.text = "Error sending recovery email: " + error.ErrorMessage;
                Debug.LogError("Error sending recovery email: " + error.GenerateErrorReport());

            });

    }



}
