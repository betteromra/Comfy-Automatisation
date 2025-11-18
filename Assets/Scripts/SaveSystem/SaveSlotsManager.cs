using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages all save slot UI elements
/// </summary>
public class SaveSlotsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject saveSlotsPanel;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button closePanelButton;
    
    [Header("Settings")]
    [SerializeField] private bool showOnStart = false;
    [SerializeField] private int numberOfSlots = 5;
    
    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();
    
    private void Start()
    {
        InitializeSlots();
        
        if (saveSlotsPanel != null)
            saveSlotsPanel.SetActive(showOnStart);
        
        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(HidePanel);
        
        // Subscribe to save system events
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGameSaved += RefreshAllSlots;
            SaveSystem.Instance.OnGameLoaded += RefreshAllSlots;
        }
    }
    
    private void OnDestroy()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGameSaved -= RefreshAllSlots;
            SaveSystem.Instance.OnGameLoaded -= RefreshAllSlots;
        }
    }
    
    private void InitializeSlots()
    {
        if (slotsContainer == null || saveSlotPrefab == null)
        {
            //Debug.LogError("SaveSlotsManager: Missing required references!");
            return;
        }
        
        // Clear existing slots
        foreach (var slot in slotUIs)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        slotUIs.Clear();
        
        // Create slot UIs
        for (int i = 1; i <= numberOfSlots; i++)
        {
            GameObject slotObj = Instantiate(saveSlotPrefab, slotsContainer);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Initialize(i);
                slotUIs.Add(slotUI);
            }
            else
            {
                //Debug.LogError($"SaveSlotPrefab is missing SaveSlotUI component!");
            }
        }
    }
    
    public void ShowPanel()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(true);
            RefreshAllSlots();
        }
    }
    
    public void HidePanel()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(false);
        }
    }
    
    public void TogglePanel()
    {
        if (saveSlotsPanel != null)
        {
            if (saveSlotsPanel.activeSelf)
                HidePanel();
            else
                ShowPanel();
        }
    }
    
    private void RefreshAllSlots()
    {
        foreach (var slotUI in slotUIs)
        {
            if (slotUI != null)
                slotUI.RefreshDisplay();
        }
    }
    
    /// <summary>
    /// Save to next available slot
    /// </summary>
    public void SaveToNextAvailableSlot()
    {
        if (SaveSystem.Instance == null)
            return;
        
        // Find first empty slot
        for (int i = 1; i <= numberOfSlots; i++)
        {
            if (!SaveSystem.Instance.IsSlotOccupied(i))
            {
                SaveSystem.Instance.SaveToSlot(i);
                RefreshAllSlots();
                return;
            }
        }
        
        // If all slots full, save to slot 1
        //Debug.Log("All slots full, overwriting slot 1");
        SaveSystem.Instance.SaveToSlot(1);
        RefreshAllSlots();
    }
}
