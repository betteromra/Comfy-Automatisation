using System;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    private const string SAVE_FOLDER = "Saves";
    private const string SAVE_EXTENSION = ".json";
    private const string SAVE_SLOT_PREFIX = "slot_";
    private const int MAX_SAVE_SLOTS = 5;
    private string savePath;
    
    public event Action OnGameSaved;
    public event Action OnGameLoaded;
    public event Action<int> OnSlotSaved; // Passes slot number
    public event Action<int> OnSlotLoaded; // Passes slot number
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Set save path
        savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        
        // Create save folder if it doesn't exist
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created save folder at: {savePath}");
        }
    }
    
    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame(string saveName = "default")
    {
        try
        {
            SaveData saveData = new SaveData();
            saveData.saveName = saveName;
            saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            // Collect all data
            CollectPlayerData(ref saveData);
            CollectInventoryData(ref saveData);
            CollectBuildingData(ref saveData);
            CollectQuestData(ref saveData);
            CollectWorldData(ref saveData);
            
            // Convert to JSON
            string json = JsonUtility.ToJson(saveData, true);
            
            // Write to file
            string fileName = saveName + SAVE_EXTENSION;
            string fullPath = Path.Combine(savePath, fileName);
            File.WriteAllText(fullPath, json);
            
            Debug.Log($"Game saved to: {fullPath}");
            OnGameSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
    
    /// <summary>
    /// Save to a specific slot (1-5)
    /// </summary>
    public void SaveToSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid slot number: {slotNumber}. Must be between 1 and {MAX_SAVE_SLOTS}");
            return;
        }
        
        string slotName = SAVE_SLOT_PREFIX + slotNumber;
        SaveGame(slotName);
        OnSlotSaved?.Invoke(slotNumber);
    }
    
    /// <summary>
    /// Load from a specific slot (1-5)
    /// </summary>
    public bool LoadFromSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid slot number: {slotNumber}. Must be between 1 and {MAX_SAVE_SLOTS}");
            return false;
        }
        
        string slotName = SAVE_SLOT_PREFIX + slotNumber;
        bool success = LoadGame(slotName);
        if (success)
        {
            OnSlotLoaded?.Invoke(slotNumber);
        }
        return success;
    }
    
    /// <summary>
    /// Check if a slot has a save
    /// </summary>
    public bool IsSlotOccupied(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            return false;
        
        string slotName = SAVE_SLOT_PREFIX + slotNumber;
        return SaveExists(slotName);
    }
    
    /// <summary>
    /// Get save data info for a slot without loading it
    /// </summary>
    public SaveData GetSlotInfo(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            return null;
        
        try
        {
            string slotName = SAVE_SLOT_PREFIX + slotNumber;
            string fileName = slotName + SAVE_EXTENSION;
            string fullPath = Path.Combine(savePath, fileName);
            
            if (!File.Exists(fullPath))
                return null;
            
            string json = File.ReadAllText(fullPath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read slot {slotNumber} info: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Delete a save slot
    /// </summary>
    public bool DeleteSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            return false;
        
        string slotName = SAVE_SLOT_PREFIX + slotNumber;
        return DeleteSave(slotName);
    }
    
    /// <summary>
    /// Load a saved game
    /// </summary>
    public bool LoadGame(string saveName = "default")
    {
        try
        {
            string fileName = saveName + SAVE_EXTENSION;
            string fullPath = Path.Combine(savePath, fileName);
            
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Save file not found: {fullPath}");
                return false;
            }
            
            // Read JSON
            string json = File.ReadAllText(fullPath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("Failed to parse save data");
                return false;
            }
            
            // Apply all data
            ApplyPlayerData(saveData);
            ApplyInventoryData(saveData);
            ApplyBuildingData(saveData);
            ApplyQuestData(saveData);
            ApplyWorldData(saveData);
            
            Debug.Log($"Game loaded from: {fullPath}");
            OnGameLoaded?.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Delete a save file
    /// </summary>
    public bool DeleteSave(string saveName)
    {
        try
        {
            string fileName = saveName + SAVE_EXTENSION;
            string fullPath = Path.Combine(savePath, fileName);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Debug.Log($"Deleted save: {fullPath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Save file not found: {fullPath}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Check if a save exists
    /// </summary>
    public bool SaveExists(string saveName)
    {
        string fileName = saveName + SAVE_EXTENSION;
        string fullPath = Path.Combine(savePath, fileName);
        return File.Exists(fullPath);
    }
    
    /// <summary>
    /// Get all available save files
    /// </summary>
    public string[] GetAllSaves()
    {
        if (!Directory.Exists(savePath))
            return new string[0];
        
        string[] files = Directory.GetFiles(savePath, "*" + SAVE_EXTENSION);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        return files;
    }
    
    #region Data Collection
    
    private void CollectPlayerData(ref SaveData saveData)
    {
        Player player = GameManager.instance?.player;
        if (player != null)
        {
            saveData.playerData.position = new Vector3Data(player.transform.position);
            saveData.playerData.rotation = new Vector3Data(player.transform.eulerAngles);
        }
        
        // Collect unlocked buildings from BuildingManager
        BuildingManager buildingManager = GameManager.instance?.buildingManager;
        if (buildingManager != null)
        {
            // You may need to add a way to track unlocked buildings
            // For now, we'll save all building types that have been created
            foreach (var building in buildingManager.buildingsCreated)
            {
                if (building != null && building.buildingSO != null)
                {
                    string buildingName = building.buildingSO.name;
                    if (!saveData.playerData.unlockedBuildings.Contains(buildingName))
                    {
                        saveData.playerData.unlockedBuildings.Add(buildingName);
                    }
                }
            }
        }
    }
    
    private void CollectInventoryData(ref SaveData saveData)
    {
        BuildingManager buildingManager = GameManager.instance?.buildingManager;
        if (buildingManager?.barn?.inventory != null)
        {
            Inventory inventory = buildingManager.barn.inventory;
            
            // Save resources
            foreach (var kvp in inventory.ressourcesStored)
            {
                if (kvp.Key != null && kvp.Value > 0)
                {
                    saveData.inventoryData.resources.Add(new ResourceEntry(kvp.Key.name, kvp.Value));
                }
            }
            
            //saveData.inventoryData.maxWeight = inventory.weight;
            saveData.inventoryData.maxSameResourceSpace = inventory.maxSameRessourceSpace;
        }
    }
    
    private void CollectBuildingData(ref SaveData saveData)
    {
        BuildingManager buildingManager = GameManager.instance?.buildingManager;
        if (buildingManager == null) return;
        
        foreach (var building in buildingManager.buildingsCreated)
        {
            if (building == null || building.buildingSO == null) continue;
            
            BuildingData buildingData = new BuildingData();
            buildingData.buildingSOName = building.buildingSO.name;
            buildingData.buildingType = building.GetType().Name;
            buildingData.position = new Vector3Data(building.transform.position);
            buildingData.rotation = new Vector3Data(building.transform.eulerAngles);
            buildingData.scale = new Vector3Data(building.transform.localScale);
            buildingData.isPlaced = true;
            
            // Check if it's a crafting building
            if (building is CraftBuilding craftBuilding)
            {
                buildingData.craftingData = new CraftingData();
                // Add crafting state if needed
                // buildingData.craftingData.isCrafting = craftBuilding.isCrafting;
            }
            
            // Check if it's a storage building
            if (building is StorageBuilding storageBuilding && storageBuilding.inventory != null)
            {
                buildingData.storageInventory = new InventoryData();
                foreach (var kvp in storageBuilding.inventory.ressourcesStored)
                {
                    if (kvp.Key != null && kvp.Value > 0)
                    {
                        buildingData.storageInventory.resources.Add(new ResourceEntry(kvp.Key.name, kvp.Value));
                    }
                }
            }
            
            saveData.buildings.Add(buildingData);
        }
    }
    
    private void CollectQuestData(ref SaveData saveData)
    {
        QuestManager questManager = GameManager.instance?.questManager;
        if (questManager != null)
        {
            saveData.questData.currentQuestIndex = questManager.GetCurrentQuestIndex();
            var currentQuest = questManager.GetCurrentQuest();
            if (currentQuest != null && currentQuest.goals != null)
            {
                for (int i = 0; i < currentQuest.goals.Length; i++)
                {
                    var goal = currentQuest.goals[i];
                    saveData.questData.goalProgress.Add(new QuestGoalProgress(i, goal.currentAmount, goal.isCompleted));
                }
            }
        }
    }
    
    private void CollectWorldData(ref SaveData saveData)
    {
        DayNightCycleManager dayNightManager = GameManager.instance?.dayNightCycleManager;
        if (dayNightManager != null)
        {
            saveData.currentTime = dayNightManager.GetCurrentTime();
            // You can add day counter if you implement it
        }
    }
    
    #endregion
    
    #region Data Application
    
    private void ApplyPlayerData(SaveData saveData)
    {
        Player player = GameManager.instance?.player;
        if (player != null && saveData.playerData != null)
        {
            player.transform.position = saveData.playerData.position.ToVector3();
            player.transform.eulerAngles = saveData.playerData.rotation.ToVector3();
        }
    }
    
    private void ApplyInventoryData(SaveData saveData)
    {
        BuildingManager buildingManager = GameManager.instance?.buildingManager;
        if (buildingManager?.barn?.inventory != null && saveData.inventoryData != null)
        {
            Inventory inventory = buildingManager.barn.inventory;
            
            // Clear existing inventory
            inventory.ClearInventory();
            
            // Load resources
            foreach (var resourceEntry in saveData.inventoryData.resources)
            {
                // Load the resource SO from Resources folder
                RessourceSO resource = Resources.Load<RessourceSO>($"Ressources/{resourceEntry.resourceName}");
                if (resource != null)
                {
                    RessourceAndAmount resourceAndAmount = new RessourceAndAmount(resource, resourceEntry.amount);
                    inventory.Add(resourceAndAmount);
                }
                else
                {
                    Debug.LogWarning($"Could not find resource: {resourceEntry.resourceName}");
                }
            }
        }
    }
    
    private void ApplyBuildingData(SaveData saveData)
    {
        BuildingManager buildingManager = GameManager.instance?.buildingManager;
        if (buildingManager == null || saveData.buildings == null) return;
        
        // Clear existing buildings (except essential ones like barn)
        // You may want to implement a method to clear created buildings
        
        foreach (var buildingData in saveData.buildings)
        {
            // Load building SO
            BuildingSO buildingSO = Resources.Load<BuildingSO>($"Buildings/{buildingData.buildingSOName}");
            if (buildingSO == null)
            {
                Debug.LogWarning($"Could not find building SO: {buildingData.buildingSOName}");
                continue;
            }
            
            // Create the building
            // You may need to implement a method in BuildingManager to create buildings from save data
            // For now, this is a placeholder
            // Building building = buildingManager.CreateBuildingFromSave(buildingSO, buildingData);
            
            // Apply position, rotation, scale
            // building.transform.position = buildingData.position.ToVector3();
            // building.transform.eulerAngles = buildingData.rotation.ToVector3();
            // building.transform.localScale = buildingData.scale.ToVector3();
            
            // Apply storage inventory if it's a storage building
            // if (building is StorageBuilding storageBuilding && buildingData.storageInventory != null)
            // {
            //     foreach (var resourceEntry in buildingData.storageInventory.resources)
            //     {
            //         RessourceSO resource = Resources.Load<RessourceSO>($"Ressources/{resourceEntry.resourceName}");
            //         if (resource != null)
            //         {
            //             storageBuilding.inventory.AddRessource(resource, resourceEntry.amount);
            //         }
            //     }
            // }
        }
    }
    
    private void ApplyQuestData(SaveData saveData)
    {
        QuestManager questManager = GameManager.instance?.questManager;
        if (questManager != null && saveData.questData != null)
        {
            questManager.SetCurrentQuestIndex(saveData.questData.currentQuestIndex);
            questManager.LoadQuestProgress(saveData.questData.goalProgress);
        }
    }
    
    private void ApplyWorldData(SaveData saveData)
    {
        DayNightCycleManager dayNightManager = GameManager.instance?.dayNightCycleManager;
        if (dayNightManager != null)
        {
            dayNightManager.SetTime(saveData.currentTime);
        }
    }
    
    #endregion
}
