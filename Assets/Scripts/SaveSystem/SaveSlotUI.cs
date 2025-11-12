using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI component for a single save slot
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI saveInfoText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject emptySlotPanel;
    [SerializeField] private GameObject occupiedSlotPanel;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color occupiedColor = new Color(0.5f, 0.7f, 1f);
    [SerializeField] private Image backgroundImage;
    
    private int slotNumber;
    private bool isOccupied;
    
    public void Initialize(int slot)
    {
        slotNumber = slot;
        
        if (slotNumberText != null)
            slotNumberText.text = $"Slot {slotNumber}";
        
        // Setup button listeners
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
        
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);
        
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);
        
        RefreshDisplay();
    }
    
    public void RefreshDisplay()
    {
        if (SaveSystem.Instance == null)
            return;
        
        isOccupied = SaveSystem.Instance.IsSlotOccupied(slotNumber);
        
        // Update panels
        if (emptySlotPanel != null)
            emptySlotPanel.SetActive(!isOccupied);
        
        if (occupiedSlotPanel != null)
            occupiedSlotPanel.SetActive(isOccupied);
        
        // Update background color
        if (backgroundImage != null)
            backgroundImage.color = isOccupied ? occupiedColor : emptyColor;
        
        // Update buttons
        if (loadButton != null)
            loadButton.interactable = isOccupied;
        
        if (deleteButton != null)
            deleteButton.interactable = isOccupied;
        
        // Load and display save info
        if (isOccupied)
        {
            SaveData saveData = SaveSystem.Instance.GetSlotInfo(slotNumber);
            if (saveData != null)
            {
                if (saveInfoText != null)
                {
                    // Display relevant info (customize as needed)
                    string info = $"Quest: {saveData.questData.currentQuestIndex + 1}\n";
                    info += $"Resources: {saveData.inventoryData.resources.Count} types";
                    saveInfoText.text = info;
                }
                
                if (dateText != null)
                    dateText.text = saveData.saveDate;
            }
        }
        else
        {
            if (saveInfoText != null)
                saveInfoText.text = "Empty Slot";
            
            if (dateText != null)
                dateText.text = "";
        }
    }
    
    private void OnSaveClicked()
    {
        if (SaveSystem.Instance != null)
        {
            // If slot is occupied, show confirmation (optional)
            if (isOccupied)
            {
                // For now, just overwrite
                Debug.Log($"Overwriting save in slot {slotNumber}");
            }
            
            SaveSystem.Instance.SaveToSlot(slotNumber);
            RefreshDisplay();
        }
    }
    
    private void OnLoadClicked()
    {
        if (SaveSystem.Instance != null && isOccupied)
        {
            SaveSystem.Instance.LoadFromSlot(slotNumber);
        }
    }
    
    private void OnDeleteClicked()
    {
        if (SaveSystem.Instance != null && isOccupied)
        {
            SaveSystem.Instance.DeleteSlot(slotNumber);
            RefreshDisplay();
        }
    }
}
