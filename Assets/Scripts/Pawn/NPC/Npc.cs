using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(Selectable))]
public class Npc : Pawn
{
    [SerializeField] private NpcSO _nonPlayableCharacterSO;
    [SerializeField] private NpcPathRenderer _npcPathRenderer;
    public event Action<Npc, bool> OnSelfSelected;
    private List<NodeLink> _linkedNodeList = new();
    private GameObject _tempClickTarget;
    private RessourceAndAmount _carrying;
    private BehaviorGraphAgent _behaviorAgent;
    private bool _isSelected = false;

    void Awake()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();

        _behaviorAgent.BlackboardReference.SetVariableValue("NPCSpeed", _nonPlayableCharacterSO.Speed);
        _behaviorAgent.BlackboardReference.SetVariableValue("NPCWaitDuration", _nonPlayableCharacterSO.WaitDuration);
        
        _carrying = null;

        GetComponent<Selectable>().onSelfSelected += HandleSelection;
    }

    public void LinkNode(NodeLink nodeLink)
    {
        _linkedNodeList.Add(nodeLink);
        Link(nodeLink.NodeA);
        Link(nodeLink.NodeB);

        if(_isSelected)
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);
    }

    public void UnlinkNode(NodeLink nodeLink)
    {
        bool exists = _linkedNodeList.Exists(l => l == nodeLink);

        if (!exists)
            return;

        _linkedNodeList.Remove(nodeLink);
        Unlink(nodeLink.NodeA); //Add check so the node isn't used elsewhere
        Unlink(nodeLink.NodeB); //Add check so the node isn't used elsewhere

        if(_isSelected)
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);
    }
    
    /// <summary>
    /// To be used when no GameObject is present.
    /// </summary>
    /// <param name="position"></param>
    public void Link(Vector3 position)
    {
        if (_tempClickTarget == null)
        {
            //Please forgive me for this crime of a code.
            _tempClickTarget = new("ClickTarget");
            _tempClickTarget.transform.SetParent(GameManager.instance.nonPlayableCharacter.transform, worldPositionStays: true);
        }

        for (int i = _linkedNodeList.Count - 1; i >= 0; i--)
        {
            UnlinkNode(_linkedNodeList[i]);
        }
        
        _tempClickTarget.transform.position = position;
        Link(_tempClickTarget);
    }

    /// <summary>
    /// Unlinks current target and sets it equal to provided GameObject
    /// </summary>
    /// <param name="gameObject">Position to walk</param>
    public void Unlink(GameObject gameObject)
    {
        if (_behaviorAgent.BlackboardReference.GetVariable("WalkingPoints", out BlackboardVariable<List<GameObject>> walkingPoints))
        {
            _behaviorAgent.BlackboardReference.GetVariable("WalkingPointsIndex", out BlackboardVariable<int> index);
            _behaviorAgent.BlackboardReference.GetVariable("Target", out BlackboardVariable<GameObject> target);

            int currentIndex = index.Value;
            int removeIndex = walkingPoints.Value.IndexOf(gameObject);

            if (removeIndex == currentIndex)
                currentIndex++;

            walkingPoints.Value.RemoveAt(removeIndex);

            currentIndex = Mathf.Clamp(currentIndex, 0, walkingPoints.Value.Count - 1);

            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints.Value);

            if (walkingPoints.Value.Count > 0)
            {
                GameObject nextTarget = walkingPoints.Value[currentIndex];
                _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPointsIndex", currentIndex);
                _behaviorAgent.BlackboardReference.SetVariableValue("Target", nextTarget);
            }
            else
            {
                _behaviorAgent.BlackboardReference.SetVariableValue<GameObject>("Target", null);
                _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPointsIndex", 0);
            }

        }
    }

    /// <summary>
    /// Tells the NPC to pick up a resource. Picks up max at a time.
    /// </summary>
    /// <param name="target">The target</param>
    public void PickUp(GameObject target)
    {
        if (_carrying == null)
            return;

        if (_carrying.amount >= _nonPlayableCharacterSO.MaxCarryingCapacity)
            return;

        if (!target.TryGetComponent<OutputNode>(out var outputNode))
        {
            Debug.LogWarning("NPC tried to pickup at non output node!");
            return;
        }

        Inventory inventory = outputNode.inventory;
        if (inventory == null)
        {
            Debug.LogError("OutputNode inventory is set to null!");
        }

        if (_carrying.ressourceSO == null)
        {
            //Currently the NPC chooses the resource based on amount.
            RessourceSO ressource = inventory.MostRessourceInside();
            _carrying.ressourceSO = ressource;
        }

        RessourceAndAmount ressourceAndAmount = new RessourceAndAmount(_carrying.ressourceSO);
        if (inventory.ContainsAmount(ressourceAndAmount))
        {
            while (_carrying.amount < _nonPlayableCharacterSO.MaxCarryingCapacity && 0 < inventory.Contains(_carrying.ressourceSO))
            {
                inventory.Remove(ressourceAndAmount);
                _carrying.amount++;
            }
        }

        //Debug.Log($"NPC currently carrying {_carrying.CurrenltyCarrying.actualName}, {_carrying.Amount}");
    }

    /// <summary>
    /// Calls the NPC to drop of resource. Drops of all the resource at a time.
    /// </summary>
    /// <returns>Returns information about the dropped of resource.</returns>
    public void DropOff(GameObject target)
    {
        if (_carrying == null)
            return;

        if (!target.TryGetComponent<InputNode>(out var inputNode))
        {
            Debug.LogWarning("NPC tried to drop of at non input node!");
            return;
        }

        Inventory inventory = inputNode.inventory;
        if (inventory == null)
        {
            Debug.LogError("InputNode inventory is set to null!");
        }

        if (inventory.ContainsAmount(_carrying))
            return;

        inventory.Add(_carrying);

        _carrying = null;
    }

    /// <summary>
    /// Links the GameObject to the NPC
    /// </summary>
    /// <param name="gameObject">Position to walk to</param>
    private void Link(GameObject gameObject)
    {
        if (_behaviorAgent.BlackboardReference.GetVariable("WalkingPoints", out BlackboardVariable<List<GameObject>> walkingPoints))
        {   
            //Stops the duplicate linking of same gameobject resulting in multidropof/pickup
            if (walkingPoints.Value.Count > 0 && walkingPoints.Value[^1] == gameObject)
                return;

            walkingPoints.Value.Add(gameObject);
            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", walkingPoints.Value);

            if (_behaviorAgent.BlackboardReference.GetVariable("Target", out BlackboardVariable<GameObject> target))
            {
                if (target.Value == null)
                {
                    _behaviorAgent.BlackboardReference.SetVariableValue("Target", gameObject);
                }
            }
        }
        else
        {
            // If the variable doesn't exist, create a new one
            var newList = new List<GameObject> { gameObject };
            _behaviorAgent.BlackboardReference.SetVariableValue("WalkingPoints", newList);
        }
    }

    private void HandleSelection(bool isSelected)
    {
        _isSelected = isSelected;
        _npcPathRenderer.SetVisibilityOfLineRenderer(isSelected);

        if (isSelected)
        {
            _npcPathRenderer.DrawPathBetween(_linkedNodeList);
        }

        OnSelfSelected.Invoke(this, isSelected);
    }
}
