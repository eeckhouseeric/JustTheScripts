using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Fusion;
public class StartMenuPrototype : MonoBehaviour
{
    private PlaneControls inputActions;

    [SerializeField] private GameObject defaultSelectedButton;
    [SerializeField] private GameObject loginPanel; // assign in Inspector
    [SerializeField] private TextMeshProUGUI startButtonLabel; // reference to the startMenu

    private GameObject loginInstance;
    private bool isLoggedIn = false;

    private void Awake()
    {
        inputActions = new PlaneControls();
        inputActions.UI.Enable();
        inputActions.Flight.Disable();

        // default state: show "Login"
        if(startButtonLabel != null)
        {
            startButtonLabel.text = "Login";
        }

    }

    private void OnEnable()
    {
        inputActions.UI.Enable();

        inputActions.UI.confirm.performed += OnConfirmPressed;
        inputActions.UI.cancel.performed += OnCancelPressed;

        // Set the default selected UI button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
        
       // Debug.Log($"Default selected button: {defaultSelectedButton?.name}");
    }
    private IEnumerator SelectDefaultButton()
    {
        yield return new WaitForEndOfFrame(); // Ensures UI is initialized
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
        Debug.Log($"Default selected button: {defaultSelectedButton?.name}");
    }
    private void OnDisable()
    {
        inputActions.UI.confirm.performed -= OnConfirmPressed;
        inputActions.UI.cancel.performed -= OnCancelPressed;
    }

    private void OnConfirmPressed(InputAction.CallbackContext ctx)
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        Debug.Log($"Currently selected: {selected?.name}");
        if (selected != null)
        {
            var button = selected.GetComponent<Button>();
            if (button != null)
            {
                Debug.Log($"Confirm pressed on: {selected.name}");
                button.onClick.Invoke(); // Directly invoke the button's click event
            }
            else
            {
                Debug.LogWarning($"Selected object '{selected.name}' is not a Button.");
            }
        }
        else
        {
            Debug.LogWarning("No UI element is currently selected.");
        }
    }

    private void OnCancelPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Cancel pressed — exiting game.");
        OnExitButtonClicked();
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked – showing login UI");

        if (!isLoggedIn)
        {

            if (loginInstance == null)
            {
                // Spawn under the same Canvas so it renders correctly
                var parent = transform.parent;
                loginInstance = Instantiate(loginPanel, parent);
                loginInstance.transform.SetAsLastSibling();
            }
            else
            {
                loginInstance.SetActive(true);
            }

            // Move selection into the login UI
            var firstSelectable = loginInstance.GetComponentInChildren<UnityEngine.UI.Selectable>();
            if (firstSelectable != null)
                EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);

            gameObject.SetActive(false); // Hide start menu
        }
        else
        {

            StartCoroutine(WaitForRunnerThenLoadLobby());

        }
    }

private IEnumerator WaitForRunnerThenLoadLobby()
    {
        while (NetworkBootstrapper.Runner == null || !NetworkBootstrapper.Runner.IsRunning)
        { 
            Debug.Log("[StartMenuPrototype] Waiting for NetworkRunner to be ready...");
            yield return null; // wait for next frame
        }
        // Use Fusion’s scene manager for lobby
        NetworkBootstrapper.Runner.LoadScene(SceneRef.FromIndex(2));
        FusionCallbackHandler.lastSceneIndex = 2; // Lobby scene index
        Debug.Log("[StartMenuPrototype] Loading Lobby loaded after runner became ready");
    }

    public void LoadGreyBox()
    {
        if (NetworkBootstrapper.Runner != null && NetworkBootstrapper.Runner.IsRunning)
        { 
            var greyBoxScene = SceneRef.FromIndex(3); // GreyBox scene index
            FusionCallbackHandler.SetSceneIndex(greyBoxScene.AsIndex);
            NetworkBootstrapper.Runner.LoadScene(greyBoxScene);
            Debug.Log("[StartMenuPrototype] Loading GreyBox scene.");
        } 
    
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Game closed (won’t work in editor)");
        Application.Quit();
    }

    public void SetButtonToStart()
    {


        isLoggedIn = true;
        if(startButtonLabel != null)
        {
            startButtonLabel.text = "Start";
            Debug.Log("[StartMenuPrototype] Button flipped to Start, user is logged in.");

        }

        else 
        {
        
         Debug.LogWarning("[StartMenuPrototype] start button label is not assigned in the Inspector.");

        }
    }

    public void SetButtonToPlay()
    {
        isLoggedIn = true;
        if(startButtonLabel != null)
        {
            startButtonLabel.text = "Play";
        }
        Debug.Log("[StartMenuPrototype] Button flipped to Play, user is logged in.");



    }


}
