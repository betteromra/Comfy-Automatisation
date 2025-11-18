using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple component to trigger save/load operations via keyboard shortcuts
/// Handles autosave functionality
/// </summary>
public class SaveLoadTrigger : MonoBehaviour
{
    [Header("Autosave Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes in seconds
    [SerializeField] private int autoSaveSlot = 5; // Use slot 5 for autosave
    
    [Header("Startup Settings")]
    [SerializeField] private bool loadOnStartup = false;
    [SerializeField] private int startupLoadSlot = 1;
    
    [Header("Keyboard Shortcuts")]
    [SerializeField] private Key quickSaveKey = Key.F5;
    [SerializeField] private Key quickLoadKey = Key.F9;
    [SerializeField] private int quickSaveSlot = 1; // Quick save goes to slot 1
    
    private float autoSaveTimer = 0f;
    private Keyboard keyboard;
    private bool isAutoSaving = false;
    
    private void Start()
    {
        keyboard = Keyboard.current;
        
        // Subscribe to save system events
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGameSaved += OnGameSaved;
            SaveSystem.Instance.OnGameLoaded += OnGameLoaded;
        }
        
        // Load on startup if enabled
        if (loadOnStartup && SaveSystem.Instance != null)
        {
            if (SaveSystem.Instance.IsSlotOccupied(startupLoadSlot))
            {
                SaveSystem.Instance.LoadFromSlot(startupLoadSlot);
                //Debug.Log($"Loaded save from slot {startupLoadSlot} on startup");
            }
        }
    }
    
    private void Update()
    {
        // Quick save
        if (keyboard != null && keyboard[quickSaveKey].wasPressedThisFrame)
        {
            QuickSave();
        }
        
        // Quick load
        if (keyboard != null && keyboard[quickLoadKey].wasPressedThisFrame)
        {
            QuickLoad();
        }
        
        // Auto save (non-blocking)
        if (autoSaveEnabled && !isAutoSaving)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }
    
    private void OnDestroy()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGameSaved -= OnGameSaved;
            SaveSystem.Instance.OnGameLoaded -= OnGameLoaded;
        }
    }
    
    public void QuickSave()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveToSlot(quickSaveSlot);
            //Debug.Log($"Quick save to slot {quickSaveSlot}");
        }
    }
    
    public void QuickLoad()
    {
        if (SaveSystem.Instance != null)
        {
            bool success = SaveSystem.Instance.LoadFromSlot(quickSaveSlot);
            if (success)
            {
                //Debug.Log($"Quick load from slot {quickSaveSlot}");
            }
            else
            {
                //Debug.LogWarning($"No save found in slot {quickSaveSlot}");
            }
        }
    }
    
    public void AutoSave()
    {
        if (SaveSystem.Instance != null && !isAutoSaving)
        {
            isAutoSaving = true;
            SaveSystem.Instance.SaveToSlot(autoSaveSlot);
            //Debug.Log($"Auto save to slot {autoSaveSlot}");
            isAutoSaving = false;
        }
    }
    
    public void SaveGame(int slotNumber)
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveToSlot(slotNumber);
        }
    }
    
    public void LoadGame(int slotNumber)
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadFromSlot(slotNumber);
        }
    }
    
    private void OnGameSaved()
    {
        // You can trigger UI feedback here
        //Debug.Log("Game saved successfully!");
    }
    
    private void OnGameLoaded()
    {
        // You can trigger UI feedback here
        //Debug.Log("Game loaded successfully!");
    }
}
