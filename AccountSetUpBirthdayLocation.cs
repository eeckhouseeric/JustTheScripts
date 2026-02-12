using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CountryList
{
    public List<string> countries;
}

public class AccountSetUpBirthdayLocation : MonoBehaviour
{
    [Header("Birthday Steppers")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private Button dayLeft;
    [SerializeField] private Button dayRight;

    [SerializeField] private TextMeshProUGUI monthText;
    [SerializeField] private Button monthLeft;
    [SerializeField] private Button monthRight;

    [SerializeField] private TextMeshProUGUI yearText;
    [SerializeField] private Button yearLeft;
    [SerializeField] private Button yearRight;

    [Header("Location Input (inline autocomplete)")]
    [SerializeField] private TMP_InputField locationInput;

    private int currentDay = 1;
    private int currentMonth = 0;
    private int currentYear;

    private float lastClickTime = 0f;
    private const float clickCooldown = 0.2f; // 200ms between clicks



    private readonly string[] months = {
        "January","February","March","April","May","June",
        "July","August","September","October","November","December"
    };

    private List<string> allCountries = new List<string>();

    // Aliases
    private Dictionary<string, string> countryAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "US", "United States of America" },
        { "USA", "United States of America" },
        { "United States", "United States of America" },
        { "UK", "United Kingdom" },
        { "UAE", "United Arab Emirates" },
        { "KSA", "Saudi Arabia" },
        { "RU", "Russia" },
        { "CN", "China" },
        { "JP", "Japan" }
    };

    void Start()
    {
        currentYear = DateTime.Now.Year;
        UpdateDay(); UpdateMonth(); UpdateYear();

        dayLeft.onClick.AddListener(() => { if (CanClick()) ChangeDay(-1); });
        dayRight.onClick.AddListener(() => { if (CanClick()) ChangeDay(1); });
        monthLeft.onClick.AddListener(() => { if (CanClick()) ChangeMonth(-1); });
        monthRight.onClick.AddListener(() => { if (CanClick()) ChangeMonth(1); });
        yearLeft.onClick.AddListener(() => { if (CanClick()) ChangeYear(-1); });
        yearRight.onClick.AddListener(() => { if (CanClick()) ChangeYear(1); });


        LoadCountries();

        // Hook autocomplete
        locationInput.onValueChanged.AddListener(OnLocationChanged);
    }

    // ---------------- Birthday ----------------
    private void ChangeDay(int delta)
    {
        int oldDay = currentDay;
        int oldMonth = currentMonth;
        int oldYear = currentYear;

        currentDay += delta; 
        ClampDay();
        UpdateDay();
        Debug.Log($"Changed day to {currentDay}");

        // Sanity check
        if (currentMonth != oldMonth) Debug.LogError("[BUG] Month changed when pressing Day button!");
        if (currentYear != oldYear) Debug.LogError("[BUG] Year changed when pressing Day button!");
    }

    private void ChangeMonth(int delta) 
    {
        int oldDay = currentDay;
        int oldMonth = currentMonth;
        int oldYear = currentYear;

        currentMonth = (currentMonth + delta + 12) % 12; 
        //ClampDay();
        UpdateMonth(); 
        //UpdateDay();
        Debug.Log($"Changed month to {months[currentMonth]}");

        // Sanity check
        if (currentDay != oldDay) Debug.LogError("[BUG] Day changed when pressing Month button!");
        if (currentYear != oldYear) Debug.LogError("[BUG] Year changed when pressing Month button!");
    }
    private void ChangeYear(int delta)
    {
        int oldDay = currentDay;
        int oldMonth = currentMonth;
        int oldYear = currentYear;

       // int oldDays = DateTime.DaysInMonth(currentYear, currentMonth + 1);
        currentYear += delta;
        currentYear = Math.Clamp(currentYear, 1900, DateTime.Now.Year);

        //int newDays = DateTime.DaysInMonth(currentYear, currentMonth + 1);

       // if(currentDay > newDays) currentDay = newDays;
        UpdateYear(); 
        //UpdateDay();
        Debug.Log($"Changed year to {currentYear}");

        // Sanity check
        if (currentDay != oldDay) Debug.LogError("[BUG] Day changed when pressing Year button!");
        if (currentMonth != oldMonth) Debug.LogError("[BUG] Month changed when pressing Year button!");
    }
    private void ClampDay() 
    { 
        int days = DateTime.DaysInMonth(currentYear, currentMonth + 1);
        if (currentDay > days) currentDay = days; 
        if (currentDay < 1) currentDay = 1; 
    }
    private void UpdateDay() => dayText.text = currentDay.ToString();
    private void UpdateMonth() => monthText.text = months[currentMonth];
    private void UpdateYear() => yearText.text = currentYear.ToString();

    // ---------------- Location ----------------
    private void LoadCountries()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/countries");
        if (jsonFile != null)
        {
            CountryList list = JsonUtility.FromJson<CountryList>(jsonFile.text);
            if (list != null && list.countries != null)
                allCountries = list.countries;
        }
    }

    private void OnLocationChanged(string typed)
    {
        if (string.IsNullOrWhiteSpace(typed)) return;

        // Alias normalization first
        if (countryAliases.TryGetValue(typed, out string mapped))
        {
            locationInput.text = mapped;
            locationInput.caretPosition = mapped.Length;
            return;
        }

        // Inline autocomplete from countries list
        string match = allCountries.FirstOrDefault(c => c.StartsWith(typed, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(match) && !match.Equals(typed, StringComparison.OrdinalIgnoreCase))
        {
            locationInput.text = match;
            locationInput.caretPosition = typed.Length; // caret after what user typed
            locationInput.selectionAnchorPosition = typed.Length;
            locationInput.selectionFocusPosition = match.Length; // highlight the autocompleted part
        }
    }

    private string NormalizeLocation(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        if (countryAliases.TryGetValue(input.Trim(), out string mapped))
            return mapped;
        return input.Trim();
    }

    private bool IsLocationValid(string normalized)
    {
        return allCountries.Exists(c => string.Equals(c, normalized, StringComparison.OrdinalIgnoreCase));
    }

    // ---------------- Public API ----------------
    public void OnNextButtonPressed()
    {
        string birthday = GetBirthday();
        string rawLocation = GetLocation();
        string normalizedLocation = NormalizeLocation(rawLocation);

        if (!IsLocationValid(normalizedLocation))
        {
            Debug.LogError($"[BirthdayStep] Invalid location: {rawLocation}");
            return;
        }

        AccountCreationFlow.Instance.SetBirthdayData(birthday, normalizedLocation);
        AccountCreationFlow.Instance.GoToNextStep(this);
    }

    private bool CanClick()
    {
        if (Time.time - lastClickTime < clickCooldown) return false;
        lastClickTime = Time.time;
        return true;
    }





    public string GetBirthday() => $"{months[currentMonth]} {currentDay}, {currentYear}";
    public string GetLocation() => locationInput.text;
}
