using TMPro;
using UnityEngine;

public class ResetPasswordUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField newPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private TextMeshProUGUI feedbackText;

    public void OnResetPassword()
    {
        string newPassword = newPasswordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (newPassword.Length < 12)
        {
            feedbackText.text = "Password must be at least 12 characters long.";
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"[^a-zA-Z0-9]"))
        {
            feedbackText.text = "Password must contain at least one special character.";
            return;
        }

        if (newPassword != confirmPassword)
        {
            feedbackText.text = "Passwords do not match.";
            return;
        }


        // PlayFab does not support direct password reset from Unity.
        // This is where you'd call your backend or handle a verified reset token.
       feedbackText.text = "Password reset validated locally. (Integrate with backend/PlayFab flow)";
    }
}
