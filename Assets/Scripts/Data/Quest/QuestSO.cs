using System;
using UnityEngine;

[Serializable]
public enum QuestTriggerType
{
    BuildStructure,
    CollectResource,
    ReachLocation,
    InteractWithObject
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Quest Info")]
    public string questID;
    public string questName;
    [TextArea(3, 6)]
    public string questDescription;
    
    [Header("Quest Goals")]
    public QuestGoal[] goals;
    
    [Header("Rewards")]
    public QuestReward[] rewards;
    
    [Header("Quest Flow")]
    public QuestSO nextQuest; // The quest that unlocks after this one
}

[Serializable]
public class QuestGoal
{
    public string goalDescription;
    public QuestTriggerType triggerType;
    
    [Header("Trigger Conditions")]
    public string targetID; // ID of building, resource, object, etc.
    public int requiredAmount = 1;
    public Vector3 targetLocation; // For ReachLocation type
    public float locationRadius = 5f;
    
    [HideInInspector]
    public int currentAmount = 0;
    [HideInInspector]
    public bool isCompleted = false;
}

[Serializable]
public class QuestReward
{
    public enum RewardType
    {
        Resource,
        UnlockBuilding,
        UnlockRecipe
    }
    
    public RewardType type;
    public string rewardID;
    public int amount;
}
