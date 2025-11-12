using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Game metadata
    public string saveName;
    public string saveDate;
    public int saveVersion = 1;
    
    // Player progress
    public PlayerData playerData;
    
    // Inventory
    public InventoryData inventoryData;
    
    // Buildings
    public List<BuildingData> buildings;
    
    // Quests
    public QuestProgressData questData;
    
    // World state
    public float currentTime;
    public int currentDay;
    
    public SaveData()
    {
        saveName = "Save";
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        playerData = new PlayerData();
        inventoryData = new InventoryData();
        buildings = new List<BuildingData>();
        questData = new QuestProgressData();
        currentTime = 0f;
        currentDay = 1;
    }
}

[System.Serializable]
public class PlayerData
{
    public Vector3Data position;
    public Vector3Data rotation;
    public List<string> unlockedBuildings; // Building SO names
    
    public PlayerData()
    {
        position = new Vector3Data();
        rotation = new Vector3Data();
        unlockedBuildings = new List<string>();
    }
}

[System.Serializable]
public class InventoryData
{
    public List<ResourceEntry> resources;
    public int maxWeight;
    public int maxDifferentResourceAmount;
    public int maxSameResourceSpace;
    
    public InventoryData()
    {
        resources = new List<ResourceEntry>();
        maxWeight = int.MaxValue;
        maxDifferentResourceAmount = int.MaxValue;
        maxSameResourceSpace = int.MaxValue;
    }
}

[System.Serializable]
public class ResourceEntry
{
    public string resourceName; // RessourceSO name
    public int amount;
    
    public ResourceEntry(string name, int amt)
    {
        resourceName = name;
        amount = amt;
    }
}

[System.Serializable]
public class BuildingData
{
    public string buildingSOName; // Building ScriptableObject name
    public string buildingType; // Type of building (CraftBuilding, StorageBuilding, etc.)
    public Vector3Data position;
    public Vector3Data rotation;
    public Vector3Data scale;
    public bool isPlaced; // Whether it's actually placed or just a ghost
    
    // For CraftBuilding
    public CraftingData craftingData;
    
    // For StorageBuilding
    public InventoryData storageInventory;
    
    public BuildingData()
    {
        position = new Vector3Data();
        rotation = new Vector3Data();
        scale = new Vector3Data(1, 1, 1);
        isPlaced = true;
    }
}

[System.Serializable]
public class CraftingData
{
    public string currentRecipeName; // Recipe SO name
    public float craftingProgress;
    public bool isCrafting;
    
    public CraftingData()
    {
        currentRecipeName = "";
        craftingProgress = 0f;
        isCrafting = false;
    }
}

[System.Serializable]
public class QuestProgressData
{
    public int currentQuestIndex;
    public List<QuestGoalProgress> goalProgress;
    public List<int> completedQuestIndices;
    
    public QuestProgressData()
    {
        currentQuestIndex = 0;
        goalProgress = new List<QuestGoalProgress>();
        completedQuestIndices = new List<int>();
    }
}

[System.Serializable]
public class QuestGoalProgress
{
    public int goalIndex;
    public int currentProgress;
    public bool isCompleted;
    
    public QuestGoalProgress(int index, int progress, bool completed)
    {
        goalIndex = index;
        currentProgress = progress;
        isCompleted = completed;
    }
}

[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;
    
    public Vector3Data()
    {
        x = 0f;
        y = 0f;
        z = 0f;
    }
    
    public Vector3Data(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public Vector3Data(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
