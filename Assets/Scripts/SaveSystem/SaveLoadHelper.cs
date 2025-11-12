using UnityEngine;

/// <summary>
/// Simple helper to quickly access save/load functionality from anywhere
/// Can be called from UI buttons or other scripts
/// </summary>
public class SaveLoadHelper : MonoBehaviour
{
    // Call these from UI buttons
    public void SaveSlot1() => SaveToSlot(1);
    public void SaveSlot2() => SaveToSlot(2);
    public void SaveSlot3() => SaveToSlot(3);
    public void SaveSlot4() => SaveToSlot(4);
    public void SaveSlot5() => SaveToSlot(5);
    
    public void LoadSlot1() => LoadFromSlot(1);
    public void LoadSlot2() => LoadFromSlot(2);
    public void LoadSlot3() => LoadFromSlot(3);
    public void LoadSlot4() => LoadFromSlot(4);
    public void LoadSlot5() => LoadFromSlot(5);
    
    public void DeleteSlot1() => DeleteSlot(1);
    public void DeleteSlot2() => DeleteSlot(2);
    public void DeleteSlot3() => DeleteSlot(3);
    public void DeleteSlot4() => DeleteSlot(4);
    public void DeleteSlot5() => DeleteSlot(5);
    
    // Generic methods
    public void SaveToSlot(int slotNumber)
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveToSlot(slotNumber);
        }
        else
        {
            Debug.LogError("SaveSystem not found!");
        }
    }
    
    public void LoadFromSlot(int slotNumber)
    {
        if (SaveSystem.Instance != null)
        {
            if (SaveSystem.Instance.IsSlotOccupied(slotNumber))
            {
                SaveSystem.Instance.LoadFromSlot(slotNumber);
            }
            else
            {
                Debug.LogWarning($"Slot {slotNumber} is empty!");
            }
        }
        else
        {
            Debug.LogError("SaveSystem not found!");
        }
    }
    
    public void DeleteSlot(int slotNumber)
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSlot(slotNumber);
        }
        else
        {
            Debug.LogError("SaveSystem not found!");
        }
    }
    
    // Quick save/load (F5/F9 alternatives)
    public void QuickSave()
    {
        SaveToSlot(1);
    }
    
    public void QuickLoad()
    {
        LoadFromSlot(1);
    }
}
