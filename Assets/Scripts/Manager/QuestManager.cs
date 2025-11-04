using System;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Setup")]
    [SerializeField] private QuestSO[] allQuests;
    [SerializeField] private int currentQuestIndex = 0;
    
    private QuestSO currentQuest;
    private Inventory inventory;
    private BuildingManager buildingManager;

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
        if (GameManager.instance != null)
        {
            buildingManager = GameManager.instance.buildingManager;
            if (buildingManager != null && buildingManager.barn != null)
            {
                inventory = buildingManager.barn.inventory;
            }
        }

        if (inventory != null)
        {
            inventory.onContentChange += OnInventoryChanged;
        }

        if (buildingManager != null)
        {
            buildingManager.onBuildingCreated += OnBuildingCreated;
        }

        if (allQuests != null && allQuests.Length > 0)
        {
            StartQuest(allQuests[currentQuestIndex]);
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.onContentChange -= OnInventoryChanged;
        }

        if (buildingManager != null)
        {
            buildingManager.onBuildingCreated -= OnBuildingCreated;
        }
    }

    public void StartQuest(QuestSO quest)
    {
        if (quest == null) return;

        currentQuest = quest;

        foreach (var goal in currentQuest.goals)
        {
            goal.isCompleted = false;
            goal.currentAmount = 0;
        }

        CheckForAutoCompletion();

        OnQuestStarted?.Invoke(currentQuest);

        Debug.Log($"<color=cyan>Quest Started: {currentQuest.questName}</color>");
    }

    private void CheckForAutoCompletion()
    {
        bool anyProgress = false;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;

            switch (goal.triggerType)
            {
                case QuestTriggerType.BuildStructure:
                    CheckBuildingProgress(goal);
                    break;
                case QuestTriggerType.CollectResource:
                    CheckResourceProgress(goal);
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

    private void OnInventoryChanged()
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.CollectResource) continue;

            CheckResourceProgress(goal);

            if (goal.isCompleted)
            {
                CompleteGoal(goal);
            }
        }
    }

    private void OnBuildingCreated()
    {
        if (currentQuest == null) return;

        foreach (var goal in currentQuest.goals)
        {
            if (goal.isCompleted) continue;
            if (goal.triggerType != QuestTriggerType.BuildStructure) continue;
            
            CheckBuildingProgress(goal);

            if (goal.isCompleted)
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

    private void CheckBuildingProgress(QuestGoal goal)
    {
        if (buildingManager != null)
        {
            int count = 0;
            foreach (var building in buildingManager.buildings)
            {
                if (building != null && (building.name.Contains(goal.targetID) || building.buildingSO.name == goal.targetID))
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
        if (inventory != null)
        {
            RessourceSO resourceSO = FindResourceByName(goal.targetID);
            if (resourceSO != null)
            {
                int count = inventory.ContainsHowMany(resourceSO);
                goal.currentAmount = count;
                
                if (goal.currentAmount >= goal.requiredAmount)
                {
                    goal.isCompleted = true;
                }
            }
        }
    }
    
    private RessourceSO FindResourceByName(string resourceName)
    {
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
        
        GiveRewards();

        OnQuestCompleted?.Invoke(currentQuest);

        QuestSO completedQuest = currentQuest;
        currentQuest = null;

        if (completedQuest.nextQuest != null)
        {
            StartQuest(completedQuest.nextQuest);
        }
        else
        {
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
                    Debug.Log($"Unlocked building: {reward.rewardID}");
                    break;
                case QuestReward.RewardType.UnlockRecipe:
                    Debug.Log($"Unlocked recipe: {reward.rewardID}");
                    break;
            }
        }
    }

    private void GiveResourceReward(string resourceID, int amount)
    {
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

    public QuestSO GetCurrentQuest() => currentQuest;
    public bool HasActiveQuest() => currentQuest != null;
}
