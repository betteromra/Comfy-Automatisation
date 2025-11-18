using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Simple save/load menu UI
/// </summary>
public class SaveLoadMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Transform saveListContent;
    [SerializeField] private GameObject saveEntryPrefab;
    
    [Header("Settings")]
    [SerializeField] private bool showOnStart = false;
    
    private List<SaveEntry> saveEntries = new List<SaveEntry>();
    private string selectedSave = "";
    
    private void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(showOnStart);
        }
        
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        
        RefreshSaveList();
    }
    
    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
            if (menuPanel.activeSelf)
            {
                RefreshSaveList();
            }
        }
    }
    
    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            RefreshSaveList();
        }
    }
    
    public void HideMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }
    
    private void RefreshSaveList()
    {
        // Clear existing entries
        foreach (var entry in saveEntries)
        {
            if (entry != null && entry.gameObject != null)
            {
                Destroy(entry.gameObject);
            }
        }
        saveEntries.Clear();
        
        if (SaveSystem.Instance == null || saveListContent == null || saveEntryPrefab == null)
            return;
        
        // Get all saves
        string[] saves = SaveSystem.Instance.GetAllSaves();
        
        // Create UI entries for each save
        foreach (string saveName in saves)
        {
            GameObject entryObj = Instantiate(saveEntryPrefab, saveListContent);
            SaveEntry entry = entryObj.GetComponent<SaveEntry>();
            
            if (entry != null)
            {
                entry.Initialize(saveName, this);
                saveEntries.Add(entry);
            }
        }
    }
    
    public void SelectSave(string saveName)
    {
        selectedSave = saveName;
        
        // Update input field
        if (saveNameInput != null)
        {
            saveNameInput.text = saveName;
        }
        
        // Highlight selected entry
        foreach (var entry in saveEntries)
        {
            entry.SetSelected(entry.saveName == saveName);
        }
    }
    
    private void OnSaveButtonClicked()
    {
        string saveName = saveNameInput != null ? saveNameInput.text : "default";
        
        if (string.IsNullOrWhiteSpace(saveName))
        {
            //Debug.LogWarning("Please enter a save name");
            return;
        }
        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame(saveName);
            RefreshSaveList();
        }
    }
    
    private void OnLoadButtonClicked()
    {
        string saveName = saveNameInput != null ? saveNameInput.text : selectedSave;
        
        if (string.IsNullOrWhiteSpace(saveName))
        {
            //Debug.LogWarning("Please select a save to load");
            return;
        }
        
        if (SaveSystem.Instance != null)
        {
            bool success = SaveSystem.Instance.LoadGame(saveName);
            if (success)
            {
                HideMenu();
            }
        }
    }
    
    private void OnDeleteButtonClicked()
    {
        string saveName = saveNameInput != null ? saveNameInput.text : selectedSave;
        
        if (string.IsNullOrWhiteSpace(saveName))
        {
            //Debug.LogWarning("Please select a save to delete");
            return;
        }
        
        if (SaveSystem.Instance != null)
        {
            bool success = SaveSystem.Instance.DeleteSave(saveName);
            if (success)
            {
                selectedSave = "";
                if (saveNameInput != null)
                {
                    saveNameInput.text = "";
                }
                RefreshSaveList();
            }
        }
    }
}

/// <summary>
/// Individual save entry in the list
/// </summary>
public class SaveEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image background;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.cyan;
    
    public string saveName { get; private set; }
    private SaveLoadMenu menu;
    
    public void Initialize(string name, SaveLoadMenu parentMenu)
    {
        saveName = name;
        menu = parentMenu;
        
        if (saveNameText != null)
        {
            saveNameText.text = name;
        }
        
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }
        
        SetSelected(false);
    }
    
    private void OnSelectClicked()
    {
        if (menu != null)
        {
            menu.SelectSave(saveName);
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (background != null)
        {
            background.color = selected ? selectedColor : normalColor;
        }
    }
}
