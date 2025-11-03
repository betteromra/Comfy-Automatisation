using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Setup")]
    [SerializeField] private QuestSO[] allQuests; // All quests in order
    [SerializeField] private int currentQuestIndex = 0;
    
    private QuestSO currentQuest;
    private Dictionary<string, int> progressTracker = new Dictionary<string, int>();
    private float questStartTime;

    // Events
    public event Action<QuestSO> OnQuestStarted;
    public event Action<QuestSO> OnQuestCompleted;
    public event Action<QuestGoal> OnGoalProgressUpdated;
    public event Action<QuestGoal> OnGoalCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (allQuests != null && allQuests.Length > 0)
        {
            StartQuest(allQuests[currentQuestIndex]);
        }
    }

    private void Update()
    {
        if (currentQuest != null)
        {
            CheckTimeBasedGoals();
        }
    }

    public void StartQuest(QuestSO quest)
    {
        if (quest == null) return;

        currentQuest = quest;
        questStartTime = Time.time;

        // Reset all goal progress
        foreach (var goal in currentQuest.goals)
        {
            goal.isCompleted = false;
            goal.currentAmount = 0;
        }

        // Check if any goals are already completed
        CheckForAutoCompletion();

        OnQuestStarted?.Invoke(currentQuest);

        Debug.Log($"<color=cyan>Quest Started: {currentQuest.questName}</color>");
    }

    // Check if quest goals were already accomplished before quest started
    private void CheckForAutoCompletion()
    {
        bool anyProgress = false;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;

            // Check based on trigger type
            switch (goal.triggerType)
            {
                case QuestTriggerType.BuildStructure:
                    CheckBuildingProgress(goal);
                    break;
                case QuestTriggerType.CollectResource:
                    CheckResourceProgress(goal);
                    break;
                case QuestTriggerType.Custom:
                    CheckCustomProgress(goal);
                    break;
            }

            if (goal.isCompleted)
            {
                anyProgress = true;
                OnGoalCompleted?.Invoke(goal);
                Debug.Log($"<color=yellow>Goal auto-completed: {goal.goalDescription}</color>");
            }
        }

        if (anyProgress)
        {
            CheckQuestCompletion();
        }
    }

    #region Trigger Methods

    public void TriggerBuildStructure(string buildingID)
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.BuildStructure) continue;
            if (goal.targetID != buildingID) continue;

            goal.currentAmount++;
            OnGoalProgressUpdated?.Invoke(goal);

            if (goal.currentAmount >= goal.requiredAmount)
            {
                CompleteGoal(goal);
            }
        }
    }

    public void TriggerCollectResource(string resourceID, int amount)
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.CollectResource) continue;
            if (goal.targetID != resourceID) continue;

            goal.currentAmount += amount;
            OnGoalProgressUpdated?.Invoke(goal);

            if (goal.currentAmount >= goal.requiredAmount)
            {
                CompleteGoal(goal);
            }
        }
    }

    public void TriggerReachLocation(Vector3 playerPosition)
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.ReachLocation) continue;

            float distance = Vector3.Distance(playerPosition, goal.targetLocation);
            if (distance <= goal.locationRadius)
            {
                goal.currentAmount = 1;
                CompleteGoal(goal);
            }
        }
    }

    public void TriggerInteractWithObject(string objectID)
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.InteractWithObject) continue;
            if (goal.targetID != objectID) continue;

            goal.currentAmount++;
            OnGoalProgressUpdated?.Invoke(goal);

            if (goal.currentAmount >= goal.requiredAmount)
            {
                CompleteGoal(goal);
            }
        }
    }

    public void TriggerCustom(string customKey, int value = 1)
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.Custom) continue;
            if (goal.customTriggerKey != customKey) continue;

            goal.currentAmount += value;
            OnGoalProgressUpdated?.Invoke(goal);

            if (goal.currentAmount >= goal.requiredAmount)
            {
                CompleteGoal(goal);
            }
        }
    }

    private void CheckTimeBasedGoals()
    {
        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.TimeElapsed) continue;

            float elapsedTime = Time.time - questStartTime;
            if (elapsedTime >= goal.requiredTime)
            {
                goal.currentAmount = 1;
                CompleteGoal(goal);
            }
        }
    }

    #endregion

    #region Progress Checking (for auto-completion)

    private void CheckBuildingProgress(QuestGoal goal)
    {
        // Check if building already exists in the world
        BuildingManager buildingManager = FindAnyObjectByType<BuildingManager>();
        if (buildingManager != null)
        {
            int count = 0;
            foreach (var building in buildingManager.buildings)
            {
                // Match by building SO name or type
                if (building != null && building.name.Contains(goal.targetID))
                {
                    count++;
                }
            }
            
            goal.currentAmount = count;
            
            if (goal.currentAmount >= goal.requiredAmount)
            {
                goal.isCompleted = true;
            }
        }
    }

    private void CheckResourceProgress(QuestGoal goal)
    {
        // Check if player already has the resource
        // This would integrate with your Inventory system
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            var inventory = GameManager.instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                // Find RessourceSO by name
                RessourceSO resourceSO = FindResourceByName(goal.targetID);
                if (resourceSO != null)
                {
                    int count = inventory.Contains(resourceSO);
                    goal.currentAmount = count;
                    
                    if (goal.currentAmount >= goal.requiredAmount)
                    {
                        goal.isCompleted = true;
                    }
                }
            }
        }
    }
    
    private RessourceSO FindResourceByName(string resourceName)
    {
        // Load all RessourceSO from Resources or use a registry
        // This is a simple implementation that loads all resources
        RessourceSO[] allResources = Resources.LoadAll<RessourceSO>("ScriptableObjects");
        foreach (var resource in allResources)
        {
            if (resource.name == resourceName || resource.actualName == resourceName)
            {
                return resource;
            }
        }
        return null;
    }

    private void CheckCustomProgress(QuestGoal goal)
    {
        // Check custom progress from saved data
        if (progressTracker.ContainsKey(goal.customTriggerKey))
        {
            goal.currentAmount = progressTracker[goal.customTriggerKey];
            
            if (goal.currentAmount >= goal.requiredAmount)
            {
                goal.isCompleted = true;
            }
        }
    }

    #endregion

    private void CompleteGoal(QuestGoal goal)
    {
        goal.isCompleted = true;
        OnGoalCompleted?.Invoke(goal);
        
        Debug.Log($"<color=green>Goal Completed: {goal.goalDescription}</color>");

        CheckQuestCompletion();
    }

    private void CheckQuestCompletion()
    {
        if (currentQuest == null) return;

        // Check if all goals are completed
        bool allGoalsComplete = true;
        foreach (var goal in currentQuest.goals)
        {
            if (!goal.isCompleted)
            {
                allGoalsComplete = false;
                break;
            }
        }

        if (allGoalsComplete)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        Debug.Log($"<color=lime>Quest Completed: {currentQuest.questName}</color>");
        
        // Give rewards
        GiveRewards();

        OnQuestCompleted?.Invoke(currentQuest);

        // Move to next quest
        QuestSO completedQuest = currentQuest;
        currentQuest = null;

        // Start next quest if available
        if (completedQuest.nextQuest != null)
        {
            StartQuest(completedQuest.nextQuest);
        }
        else
        {
            // Check if there's a next quest in the array
            currentQuestIndex++;
            if (currentQuestIndex < allQuests.Length)
            {
                StartQuest(allQuests[currentQuestIndex]);
            }
            else
            {
                Debug.Log("<color=orange>All quests completed!</color>");
            }
        }
    }

    private void GiveRewards()
    {
        if (currentQuest.rewards == null || currentQuest.rewards.Length == 0) return;

        foreach (var reward in currentQuest.rewards)
        {
            switch (reward.type)
            {
                case QuestReward.RewardType.Resource:
                    GiveResourceReward(reward.rewardID, reward.amount);
                    break;
                case QuestReward.RewardType.UnlockBuilding:
                    UnlockBuilding(reward.rewardID);
                    break;
                case QuestReward.RewardType.UnlockRecipe:
                    UnlockRecipe(reward.rewardID);
                    break;
                case QuestReward.RewardType.Custom:
                    // Handle custom rewards
                    Debug.Log($"Custom reward: {reward.rewardID}");
                    break;
            }
        }
    }

    private void GiveResourceReward(string resourceID, int amount)
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            var inventory = GameManager.instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                RessourceSO resourceSO = FindResourceByName(resourceID);
                if (resourceSO != null)
                {
                    RessourceAndAmount reward = new RessourceAndAmount(resourceSO, amount);
                    inventory.Add(reward);
                    Debug.Log($"Reward: {amount}x {resourceSO.actualName}");
                }
                else
                {
                    Debug.LogWarning($"Could not find resource: {resourceID}");
                }
            }
        }
    }

    private void UnlockBuilding(string buildingID)
    {
        // Integrate with your building unlock system
        Debug.Log($"Unlocked building: {buildingID}");
    }

    private void UnlockRecipe(string recipeID)
    {
        // Integrate with your recipe unlock system
        Debug.Log($"Unlocked recipe: {recipeID}");
    }

    #region Public Getters

    public QuestSO GetCurrentQuest() => currentQuest;

    public QuestGoal[] GetCurrentGoals()
    {
        return currentQuest?.goals;
    }

    public bool IsQuestActive() => currentQuest != null;

    public float GetQuestProgress()
    {
        if (currentQuest == null || currentQuest.goals.Length == 0) return 0f;

        int completedGoals = 0;
        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) completedGoals++;
        }

        return (float)completedGoals / currentQuest.goals.Length;
    }

    #endregion

    #region Save/Load Support

    public void SaveProgress(string key, int value)
    {
        progressTracker[key] = value;
    }

    public int GetSavedProgress(string key)
    {
        return progressTracker.ContainsKey(key) ? progressTracker[key] : 0;
    }

    #endregion
}
